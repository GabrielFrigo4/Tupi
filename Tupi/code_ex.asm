extern printf: proc

.data
ms db "ola mundo", 0

.code
main proc
	sub rsp, 28h	;Reserve the shadow space
	lea rcx, ms
	call printf
	mov rax, 0
	add rsp, 28h	;Remove shadow space
	ret
main endp
End