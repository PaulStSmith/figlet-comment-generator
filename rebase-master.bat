@echo off
setlocal

:: Store the current branch name
for /f "tokens=*" %%i in ('git rev-parse --abbrev-ref HEAD') do set CURRENT_BRANCH=%%i

:: Bail if already on master
if "%CURRENT_BRANCH%"=="master" (
    echo Already on master. Nothing to rebase.
    exit /b 1
)

echo Current branch: %CURRENT_BRANCH%

:: Switch to master and pull
echo Switching to master...
git checkout master || exit /b 1

echo Pulling latest master...
git pull || exit /b 1

:: Switch back and rebase
echo Switching back to %CURRENT_BRANCH%...
git checkout %CURRENT_BRANCH% || exit /b 1

echo Rebasing %CURRENT_BRANCH% onto master...
git rebase master || exit /b 1

echo Done! %CURRENT_BRANCH% is now rebased onto master.

endlocal