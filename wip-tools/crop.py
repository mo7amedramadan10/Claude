# -*- coding: utf-8 -*-
"""قصّ صورة فوتوغرافية من شريحة الأصل إلى assets النموذج.
الاستخدام: python3 -I crop.py <key> <srcFolder/slide-NN.jpg> <x> <y> <w> <h> <name> [png]
يحفظ jpg (جودة 88) بعرض ≤1600 وحجم ≤400KB، أو png عند طلب الشفافية."""
import os, sys
from PIL import Image
key, src, x, y, w, h, name = sys.argv[1:8]
fmt = sys.argv[8] if len(sys.argv) > 8 else 'jpg'
x, y, w, h = map(int, (x, y, w, h))
im = Image.open(os.path.join('/home/user/slides-source', src)).crop((x, y, x + w, y + h))
if im.width > 1600: im = im.resize((1600, round(im.height * 1600 / im.width)), Image.LANCZOS)
out = os.path.join('/home/user/claude/backend/ChatToDashboard.Api/wwwroot/templates/freeform', key, 'assets', name + '.' + fmt)
os.makedirs(os.path.dirname(out), exist_ok=True)
if fmt == 'png': im.save(out, 'PNG', optimize=True)
else:
    q = 88
    while True:
        im.convert('RGB').save(out, 'JPEG', quality=q, optimize=True, progressive=True)
        if os.path.getsize(out) <= 400 * 1024 or q <= 50: break
        q -= 8
print(out, im.size, os.path.getsize(out) // 1024, 'KB')
