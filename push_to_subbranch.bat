xcopy /s/e /y "%cd%/qudicalwiki/result" "%cd%/../qudicalwiki_result"
cd ../qudicalwiki_result
git add .
git commit
git push