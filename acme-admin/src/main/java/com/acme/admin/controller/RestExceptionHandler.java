package com.acme.admin.controller;

import com.acme.admin.validation.ApiValidationException;
import com.acme.admin.validation.ValidationIssue;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

import java.util.List;
import java.util.Map;

@ControllerAdvice
public class RestExceptionHandler {
  @ExceptionHandler(ApiValidationException.class)
  public ResponseEntity<?> handle(ApiValidationException ex) {
    final List<ValidationIssue> issues = ex.getIssues();
    return ResponseEntity.unprocessableEntity().body(Map.of("issues", issues));
  }
}
