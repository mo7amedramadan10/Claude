#!/usr/bin/env bash
# إغلاق دفعة: إعادة فحص كل مفتاح، تجميع صورة مراجعة، ثم التزام واحد ودفع. الاستخدام: finalize.sh "<label>" key1 key2 ...
set -u
SP=/tmp/claude-0/-home-user/7c327ac0-b846-52e1-a71d-20505f07e134/scratchpad
T=$SP/tools; REPO=/home/user/claude; FF=$REPO/backend/ChatToDashboard.Api/wwwroot/templates/freeform
label="$1"; shift
ok=(); bad=()
for k in "$@"; do
  d=$FF/$k
  if [ ! -f "$d/ar.html" ] || [ ! -f "$d/en.html" ] || [ ! -f "$d/template.json" ]; then echo "✗ $k: ملفات ناقصة"; bad+=("$k"); continue; fi
  mkdir -p "$d/assets"; [ -z "$(ls -A "$d/assets")" ] && touch "$d/assets/.gitkeep"
  if node "$T/check.js" "$k" > "$SP/shots/$k.check.log" 2>&1; then
    echo "✓ $k"; ok+=("$k")
    (cd "$SP/shots/$k" && ls cmp-*.png >/dev/null 2>&1 && convert $(ls cmp-*.png | sort) -append "review-$k.png" && mkdir -p /home/user/review && cp "review-$k.png" /home/user/review/)
  else
    echo "✗ $k: فشل الفحص — انظر $SP/shots/$k.check.log"; tail -5 "$SP/shots/$k.check.log"; bad+=("$k")
  fi
done
if [ ${#ok[@]} -gt 0 ]; then
  cd "$REPO" && for k in "${ok[@]}"; do git add "backend/ChatToDashboard.Api/wwwroot/templates/freeform/$k"; done
  git -c user.name="Claude" -c user.email="noreply@anthropic.com" commit -q -m "$(printf 'نماذج شرائح جاهزة (%s): %s\n\nكل نموذج: ar.html (RTL) + en.html (LTR) + template.json + assets، مبني وفق docs/freeform-template-spec.md،\nومفحوص بـ Playwright: بلا أخطاء JS، بلا موارد خارجية عدا Google Fonts، goToSlide وشريط المصغّرات،\nوبلا تمرير أفقي على 390px.\n\nCo-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>\nClaude-Session: https://claude.ai/code/session_01EXTKv3Tsv4yCJRNFTHJSty' "$label" "${ok[*]}")" \
    && printf '%s\n' "${ok[@]}" >> "$SP/done.txt" && git log --oneline -1
  for i in 1 2 3 4; do git -c push.negotiate=true push -u origin templates/batch-1 2>&1 | tail -1 && break; sleep $((i*10)); done
fi
echo "ok=${#ok[@]} bad=${#bad[@]} ${bad[*]:-}"
