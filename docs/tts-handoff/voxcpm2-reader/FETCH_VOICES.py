"""Fetch seven small NVCC clips by byte range. Python stdlib only; no full archive download."""
import argparse, hashlib, io, json, pathlib, struct, urllib.request, wave, zlib

def convert(original):
    with wave.open(io.BytesIO(original)) as w:
        channels, width, rate = w.getnchannels(), w.getsampwidth(), w.getframerate()
        pcm = w.readframes(w.getnframes())
    if channels != 1 or width not in (2, 3) or rate != 48000:
        raise ValueError('Source format changed; expected mono PCM16/24 48 kHz')
    out = pcm
    if width == 3:
        out = bytearray(len(pcm) // 3 * 2)
        out[0::2] = pcm[1::3]
        out[1::2] = pcm[2::3]
    result = io.BytesIO()
    with wave.open(result, 'wb') as w:
        w.setnchannels(1); w.setsampwidth(2); w.setframerate(rate); w.writeframes(out)
    return result.getvalue()

def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output', type=pathlib.Path, default=pathlib.Path(__file__).resolve().parent / 'voices' / 'wav')
    args = parser.parse_args()
    manifest = json.loads((pathlib.Path(__file__).resolve().parent / 'voices' / 'fetch-ranges.json').read_text(encoding='utf-8'))
    args.output.mkdir(parents=True, exist_ok=True)
    for clip in manifest['clips']:
        target = args.output / clip['file']
        if target.exists():
            print('Keeping existing file:', target); continue
        offset = clip['archive_byte_offset']; count = clip['compressed_bytes'] + 1024
        req = urllib.request.Request(manifest['source'], headers={'Range': f'bytes={offset}-{offset+count-1}'})
        with urllib.request.urlopen(req, timeout=45) as response:
            if response.status != 206 or not response.headers.get('Content-Range', '').startswith(f'bytes {offset}-'):
                raise RuntimeError('Server did not honor byte range; refusing full archive download')
            raw = response.read(count + 1)
        if len(raw) > count or raw[:4] != b'PK\x03\x04':
            raise ValueError('Archive changed or invalid ZIP member')
        name_len, extra_len = struct.unpack_from('<HH', raw, 26)
        member = raw[30:30+name_len].decode('utf-8')
        if member != clip['archive_member']:
            raise ValueError('Archive member identity changed')
        start = 30 + name_len + extra_len
        packed = raw[start:start+clip['compressed_bytes']]
        if clip['compression'] != 8: raise ValueError('Unexpected compression')
        original = zlib.decompress(packed, -15)
        if len(original) != clip['uncompressed_bytes'] or f'{zlib.crc32(original):08x}' != clip['original_crc32']:
            raise ValueError('Source integrity check failed')
        wav = convert(original)
        temporary = target.with_suffix('.part')
        temporary.write_bytes(wav); temporary.replace(target)
        print(target.name, len(wav), 'bytes', 'SHA256', hashlib.sha256(wav).hexdigest())
    print('Done. Original source CRCs verified. WAV metadata differs from the earlier FFmpeg ZIP; audio PCM is preserved.')

if __name__ == '__main__': main()
