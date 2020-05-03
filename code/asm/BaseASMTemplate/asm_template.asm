.586
.model flat, stdcall   
option casemap :none   
 
include      \masm32\include\windows.inc
include      \masm32\include\kernel32.inc
includelib   \masm32\lib\kernel32.lib
 
include      \masm32\include\user32.inc
includelib   \masm32\lib\user32.lib     

; To get unicode support
include      \masm32\macros\macros.asm

.data
; WSTR gets you a unicode string definition
;WSTR wstrTitle, "Hello"
;WSTR wstrMessage, "World"

.code

main:
   ;invoke MessageBoxW, NULL, ADDR wstrMessage, ADDR wstrTitle, MB_OK

   ;invoke ExitProcess, eax



end main