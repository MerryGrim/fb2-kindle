# Native installer bootstrap

The FB2 Kindle language selector in `source/installer/bootstrap.cpp` is project code
covered by the root MIT license. It does not change the licenses of FB2 Kindle,
Inno Setup, or the application runtimes.

The Windows x64 bootstrap is built with Microsoft Visual C++ 14.44.35207 using
`/MT`. The Microsoft C++ Standard Library portions are copyright Microsoft
Corporation and use Apache-2.0 WITH LLVM-exception; the unmodified upstream
license is included as `Microsoft-STL-LICENSE.txt` (release branch
`vs-2022-17.14`). The build receipt records the actual compiler and source hashes.

Microsoft's compiler and other C/C++ runtime portions have their own Microsoft
terms. Toolchain licensing and the redistributable-code list are published at:

- https://visualstudio.microsoft.com/license-terms/vs2022-ga-diagnosticbuildtools/
- https://learn.microsoft.com/en-us/visualstudio/releases/2022/redistribution

The compiler, SDK, development tools and a separate Visual C++ Redistributable
installer are not bundled with FB2 Kindle. The static runtime lets the language
selector start without installing .NET, Java or the Visual C++ Redistributable.
Windows system DLLs are supplied by Windows. No license here imposes additional
restrictions on the project's MIT source or requires users to obtain a compiler
to run the application.
