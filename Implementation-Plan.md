🏁 AuraMacro: Implementation Plan

1. Core Architecture

To keep the app fast and "free," we will avoid heavy web-wrappers.



Language: C# / .NET 8 (Best for Windows Hooks and UI performance).

UI Framework: WinUI 3 (Fluent Design, native Windows 11 look).

Vision Engine: Windows.Media.Ocr (Native, fast, and 0% extra CPU/License cost).

Logic Engine: JSON-based Workflow (Easy for users to share scripts).

2. Phase 1: The "Invisible" Engine (Weeks 1-2)

Low-Level Hooks: Implement SetWindowsHookEx to capture global mouse and keyboard events without the app being focused.

Action Recorder: Create a "Capture Buffer" that stores events as Action objects (e.g., ClickAt(x, y), KeyPress(key)).

Virtual Input: Use SendInput (Win32 API) for execution to ensure the macro bypasses most app-level restrictions.

3. Phase 2: The "Brain" & OCR (Weeks 3-4)

This is where the "complex" features live.



OCR Integration: Implement a "Screen Snipper" tool that sends a bitmap to the native Windows OCR engine.

Logic Gates:

Wait For Image: Scans screen for a sub-image (OpenCV).

Wait For Text: Continuously runs OCR on a region until a string appears.

If/Else: Branching logic based on the success of the above.

Variables: Allow users to store OCR text into a variable (e.g., {price_value}) to use in later steps.

4. Phase 3: The Dashboard UI (Weeks 5-6)

A macro tool is only as good as its editor.



Visual Timeline: A vertical list of "Action Blocks" that can be dragged and dropped.

The "Ghost" Overlay: A transparent window that allows users to "draw" on their screen to define OCR zones or click-targets.

Hotkeys: Global start/stop/pause commands (e.g., Ctrl + F9).

5. Phase 4: Reliability & Export (Weeks 7-8)

Error Handling: "On Failure" actions (e.g., if image not found, stop macro or alert user).

Portable Runner: The ability to export a macro as a standalone .aura file that others can run.

Resource Throttling: Ensure OCR isn't running 60 times a second; implement a "Cool-down" slider for users.