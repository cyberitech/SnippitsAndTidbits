
asm_stuff2 PROC
	push ebp
	mov ebp,esp
	sub esp,16
	and spl,-8
	mov ecx,[ebp+4]
	call puts
	mov al, key
	push eax
	mov eax,plaintextLen
	push eax
	mov eax,[ebp+4]
	push eax
	call xorStringAutokey
	mov ecx,eax
	call puts
	mov al, key
	push eax
	mov eax,plaintextLen
	push eax
	mov eax,[ebp+4]
	push eax
	call xorStringAutokey
	mov ecx,eax
	call puts
	leave
	ret
asm_stuff2 ENDP

xorString PROC ;  void xorString(char* string, uint_64 len, byte key) 
	push ebp
	mov ebp,esp
	push eax
	push ebx
	push ecx
	mov eax,[esp+20]  ;eax:=char* string //CORRECT
	mov ecx,DWORD PTR [esp+24]  ;ecx:=uint_64 len  //CORRECT
	mov bl,BYTE PTR [esp+28]  ;ebx:=byte key //CORRECT
	push edx
	xor edx,edx
xorLoop:
	mov dl,BYTE PTR [eax]  ;move the byte pointed to by eax into dl
	xor dl,bl		;xor it
	mov [eax],dl	;move the xored byte back into the byte pointed to by eax
	inc eax			;increment eax by 1
	mov edx,counter	;move the counter into edx
	inc edx			;increment it
	mov counter,edx
	cmp edx,ecx		;check if the lengths are equal
	jne xorLoop		;if not equal, repeat
DoneXor:			;clean up and leave
	pop edx	
	pop ebx
	pop ecx
	pop eax
	mov esp,ebp
	pop ebp
	ret
xorString ENDP

xorStringAutokey PROC ;  void xorStringAutokey(char* string, uint_64 len, byte key) 
	push ebp
	mov ebp,esp
	push eax
	push ebx
	push ecx
	mov eax,[esp+20]  ;eax:=char* string //CORRECT
	mov ecx,DWORD PTR [esp+24]  ;ecx:=uint_64 len  //CORRECT
	mov bl,BYTE PTR [esp+28]  ;ebx:=byte key //CORRECT
	push edx
	xor edx,edx
xorLoopAutokey:
	mov dl,BYTE PTR [eax]  ;move the byte pointed to by eax into dl
	xor dl,bl		;xor it
	mov [eax],dl	;move the xored byte back into the byte pointed to by eax
	inc eax			;increment eax by 1
	mov edx,counter	;move the counter into edx
	inc edx			;increment it
	add bl,7		;change the key... GCD(256,7)=1
	mov counter,edx
	cmp edx,ecx		;check if the lengths are equal
	jne xorLoopAutokey		;if not equal, repeat
doneXorLoopAutokey:			;clean up and leave
	xor eax,eax
	mov counter,eax
	pop edx	
	pop ebx
	pop ecx
	pop eax
	mov esp,ebp
	pop ebp
	ret
xorStringAutokey ENDP
