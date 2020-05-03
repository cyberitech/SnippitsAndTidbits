.model small
PUBLIC asm_stuff
EXTERN puts:PROC
.data
	plaintext	DB "this is my message to encrypt im going to send it through an auto-keyed xor cipher and see what it becomes.  Im then going to decrypt the message and see if it wreturns to the original message"
				DB "this is my message to encrypt im going to send it through an auto-keyed xor cipher and see what it becomes.  Im then going to decrypt the message and see if it wreturns to the original message"
				DB "this is my message to encrypt im going to send it through an auto-keyed xor cipher and see what it becomes.  Im then going to decrypt the message and see if it wreturns to the original message"
				DB "this is my message to encrypt im going to send it through an auto-keyed xor cipher and see what it becomes.  Im then going to decrypt the message and see if it wreturns to the original message",13,10,0 
	plaintextLen DWORD 762
	key BYTE 10
	counter DWORD 0
.code

asm_stuff PROC
	push ebp
	mov ebp,esp
	sub esp,16
	and spl,-8
	lea ecx,plaintext
	call puts
	mov al, key
	push eax
	mov eax,plaintextLen
	push eax
	lea eax,plaintext
	push eax
	call xorStringAutokey
	mov ecx,eax
	call puts
	mov al, key
	push eax
	mov eax,plaintextLen
	push eax
	lea eax,plaintext
	push eax
	call xorStringAutokey
	mov ecx,eax
	call puts
	leave
	ret
asm_stuff ENDP


END