#!/bin/bash

./swig -csharp -c++ -w314,315,401,451,503,516 \
    -I../../../include \
    -I../../../src \
    -I../../../src/main \
    -I../../../contrib \
    -I../../../contrib/vcpkg/installed/x86-windows-static/include \
    -outfile DotNetTypes/mqinterface.cs mqinterface.i
