package com.acme.admin.api;

import com.stripe.exception.SignatureVerificationException;
import com.stripe.exception.StripeException;

import jakarta.servlet.http.HttpServletRequest;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.http.*;
import org.springframework.web.bind.annotation.*;

import java.util.Map;

@RestControllerAdvice
public final class ApiExceptionMapper {
    
    private static final Logger log = LoggerFactory.getLogger(ApiExceptionMapper.class);

    @ExceptionHandler(IllegalArgumentException.class)
    public ResponseEntity<ApiError> onIllegalArgument(IllegalArgumentException e, HttpServletRequest req) {
        final ApiError body = ApiError.of(
            "unprocessable_entity",
            e.getMessage() == null ? "Invalid request" : e.getMessage(),
            422,
            req.getRequestURI(),
            Map.of()
        );
        return ResponseEntity.status(HttpStatus.UNPROCESSABLE_ENTITY).body(body);
    }

    @ExceptionHandler(IllegalStateException.class)
    public ResponseEntity<ApiError> onIllegalState(IllegalStateException e, HttpServletRequest req) {
        final ApiError body = ApiError.of(
            "conflict",
            e.getMessage() == null ? "Conflict" : e.getMessage(),
            409,
            req.getRequestURI(),
            Map.of()
        );
        return ResponseEntity.status(HttpStatus.CONFLICT).body(body);
    }

    @ExceptionHandler(SignatureVerificationException.class)
    public ResponseEntity<ApiError> onStripeSig(SignatureVerificationException e, HttpServletRequest req) {
        final ApiError body = ApiError.of(
            "stripe_signature_invalid",
            "Invalid webhook signature",
            400,
            req.getRequestURI(),
            Map.of()
        );
        return ResponseEntity.status(HttpStatus.BAD_REQUEST).body(body);
    }

    @ExceptionHandler(StripeException.class)
    public ResponseEntity<ApiError> onStripe(StripeException e, HttpServletRequest req) {
        final ApiError body = ApiError.of(
            "stripe_error",
            e.getMessage() == null ? "Payment provider error" : e.getMessage(),
            502,
            req.getRequestURI(),
            Map.of("type", e.getClass().getSimpleName())
        );
        return ResponseEntity.status(HttpStatus.BAD_GATEWAY).body(body);
    }

    @ExceptionHandler(Exception.class)
    public ResponseEntity<ApiError> onAny(Exception e, HttpServletRequest req) {
        log.error("Unexpected error at {}: {}", req.getRequestURI(), e.getMessage(), e);
        
        final ApiError body = ApiError.of(
            "internal_error",
            "An unexpected error occurred",
            500,
            req.getRequestURI(),
            Map.of()
        );
        return ResponseEntity.status(HttpStatus.INTERNAL_SERVER_ERROR).body(body);
    }
}
