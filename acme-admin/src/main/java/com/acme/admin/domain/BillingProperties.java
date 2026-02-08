package com.acme.admin.domain;

import org.springframework.boot.context.properties.ConfigurationProperties;

@ConfigurationProperties(prefix = "billing.stripe")
public record BillingProperties(
    String secretKey,
    String webhookSecret,
    String successUrl,
    String cancelUrl
) {}
