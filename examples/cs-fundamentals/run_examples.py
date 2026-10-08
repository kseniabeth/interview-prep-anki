"""Compile and execute exact displayed snippets. Requires the official .NET 8 SDK.
Run: python examples/cs-fundamentals/run_examples.py
No packages beyond the SDK/BCL. No external service calls.
"""
import json,os,pathlib,shutil,subprocess,sys
ROOT=pathlib.Path(__file__).resolve().parent
if not shutil.which('dotnet'):
    sys.exit('NOT RUN: dotnet SDK is not available. No compilation/runtime pass is claimed.')
manifest=json.loads((ROOT/'manifest.json').read_text())
env=dict(os.environ, DOTNET_CLI_TELEMETRY_OPTOUT='1', DOTNET_NOLOGO='1')
for case in manifest:
    project=ROOT/case['project']
    build=subprocess.run(['dotnet','build',str(project),'-c','Release','--nologo','-v','quiet'],text=True,capture_output=True,env=env)
    if build.returncode:
        print(build.stdout,build.stderr);sys.exit(f"FAIL build {case['guideId']}")
    run=subprocess.run(['dotnet','run','--project',str(project),'-c','Release','--no-build'],text=True,capture_output=True,env=env,timeout=60)
    if run.returncode or run.stdout.strip()!=case['expectedOutput']:
        print(run.stdout,run.stderr);sys.exit(f"FAIL output {case['guideId']}; expected {case['expectedOutput']!r}")
    print('PASS',case['guideId'])
print(f"PASS: {len(manifest)} self-contained .NET 8 examples compiled and executed.")
