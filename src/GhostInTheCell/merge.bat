del result.txt 2> NUL
for %%f in (*.cs) do (
   if not exist result.txt (
      copy "%%f" result.txt
   ) else (
      for /F  "usebackq skip=1 delims=" %%a in ("%%f") do (
         echo %%a>> result.txt
      )
   )
)
set /P fileDate=< result.txt