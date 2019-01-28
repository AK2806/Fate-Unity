#include <malloc.h>
#include <string.h>

void *ndk_malloc(int size) {
	return malloc(size);
}

void *ndk_realloc(void *ptr, int size) {
	return realloc(ptr, size);
}

void ndk_free(void *ptr) {
	free(ptr);
}

void *ndk_memcpy(void *dst, void *src, int count) {
	return memcpy(dst, src, count);
}

void *ndk_memmove(void *dst, void *src, int count) {
	return memmove(dst, src, count);
}

void *ndk_memset(void *ptr, int value, int count) {
	return memset(ptr, value, count);
}