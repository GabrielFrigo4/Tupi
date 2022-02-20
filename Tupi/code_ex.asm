.code
main proc
    sub rsp, 28h	;Reserve the shadow space
	xor rax, 5050
    add rsp, 28h	;Remove shadow space
    ret
main endp
End