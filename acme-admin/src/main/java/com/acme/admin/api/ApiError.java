package com.acme.admin.api;

import java.time.OffsetDateTime;
import java.util.Map;

public record ApiError(
    String code,
    String message,
    int status,
    String path,
    String at,
    Map<String, Object> details
) {
    public static ApiError of(String code, String message, int status, String path, Map<String, Object> details) {
        return new ApiError(code, message, status, path, OffsetDateTime.now().toString(), details);
    }
}
