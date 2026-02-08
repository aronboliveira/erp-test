package com.acme.admin.security.web;

import org.springframework.http.*;
import org.springframework.web.bind.MethodArgumentNotValidException;
import org.springframework.web.bind.annotation.*;

import java.util.*;

@RestControllerAdvice
public class ApiErrorAdvice {

  @ExceptionHandler(IllegalArgumentException.class)
  public ResponseEntity<Map<String, Object>> badRequest(IllegalArgumentException e) {
    return ResponseEntity.status(HttpStatus.BAD_REQUEST).body(Map.of(
      "error", "bad_request",
      "message", e.getMessage()
    ));
  }

  @ExceptionHandler(NoSuchElementException.class)
  public ResponseEntity<Map<String, Object>> notFound(NoSuchElementException e) {
    return ResponseEntity.status(HttpStatus.NOT_FOUND).body(Map.of(
      "error", "not_found",
      "message", e.getMessage()
    ));
  }

  @ExceptionHandler(MethodArgumentNotValidException.class)
  public ResponseEntity<Map<String, Object>> validation(MethodArgumentNotValidException e) {
    final var first = e.getBindingResult().getFieldErrors().stream().findFirst().orElse(null);
    final String msg = first == null ? "validation failed" : (first.getField() + ": " + first.getDefaultMessage());

    return ResponseEntity.status(HttpStatus.UNPROCESSABLE_ENTITY).body(Map.of(
      "error", "validation_error",
      "message", msg
    ));
  }
}
