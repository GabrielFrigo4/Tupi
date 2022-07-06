#include "pch.h"
#include "str.h"

EXTC char* joinStr(char* str1, char* str2) {
	int i, j;
	char* cmd;
	for (i = 0; str1[i] != '\0'; ++i);
	for (j = 0; str2[j] != '\0'; ++j);
	cmd = (char*)createMem(i + j);

	for (i = 0; str1[i] != '\0'; ++i) {
		cmd[i] = str1[i];
	}
	for (j = 0; str2[j] != '\0'; ++j, ++i) {
		cmd[i] = str2[j];
	}
	cmd[i] = '\0';
	return cmd;
}

EXTC char* joinStrWithChar(char* str1, char chr, char* str2) {
	int i, j;
	char* cmd;
	for (i = 0; str1[i] != '\0'; ++i);
	for (j = 0; str2[j] != '\0'; ++j);
	cmd = (char*)createMem(i + j + 1);

	for (i = 0; str1[i] != '\0'; ++i) {
		cmd[i] = str1[i];
	}
	cmd[i] = chr;
	i++;
	for (j = 0; str2[j] != '\0'; ++j, ++i) {
		cmd[i] = str2[j];
	}
	cmd[i] = '\0';
	return cmd;
}

EXTC char* joinStrWithSpace(char* str1, char* str2) {
	return joinStrWithChar(str1, ' ', str2);
}