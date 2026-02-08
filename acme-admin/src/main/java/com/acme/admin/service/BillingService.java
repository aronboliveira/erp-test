package com.acme.admin.service;

import com.acme.admin.domain.BillingProperties;
import com.acme.admin.dto.BillingDtos;
import com.acme.admin.provider.PaymentProvider;
import com.acme.admin.security.PermissionGate;
import org.springframework.stereotype.Service;

import java.util.List;

@Service
public class BillingService {

    private static final String PERM_CREATE_SESSION = "BILLING_CREATE_SESSION";

    private final PermissionGate gate;
    private final PaymentProvider provider;
    private final BillingProperties props;

    public BillingService(PermissionGate gate, PaymentProvider provider, BillingProperties props) {
        this.gate = gate;
        this.provider = provider;
        this.props = props;
    }

    public BillingDtos.CheckoutSessionResponse createCheckoutSession(BillingDtos.CreateCheckoutSessionRequest req) throws Exception {
        gate.assertHas(PERM_CREATE_SESSION);

        if (req == null) throw new IllegalArgumentException("billing: payload required");
        if (req.items() == null || req.items().isEmpty()) throw new IllegalArgumentException("billing: items required");

        final String currency = normalizeCurrency(req.currency());
        final var items = normalizeItems(req.items());

        final String successUrl = (req.successUrl() == null || req.successUrl().isBlank())
            ? props.successUrl()
            : req.successUrl();

        final String cancelUrl = (req.cancelUrl() == null || req.cancelUrl().isBlank())
            ? props.cancelUrl()
            : req.cancelUrl();

        if (successUrl == null || successUrl.isBlank()) throw new IllegalArgumentException("billing: successUrl required");
        if (cancelUrl == null || cancelUrl.isBlank()) throw new IllegalArgumentException("billing: cancelUrl required");

        final PaymentProvider.LineItem[] mapped = items.stream()
            .map(i -> new PaymentProvider.LineItem(i.name(), i.unitAmountCents(), i.quantity()))
            .toArray(PaymentProvider.LineItem[]::new);

        final var cmd = new PaymentProvider.CreateCheckoutSessionCommand(
            currency,
            req.customerEmail(),
            mapped,
            successUrl,
            cancelUrl
        );

        final var out = provider.createCheckoutSession(cmd);
        return new BillingDtos.CheckoutSessionResponse(out.provider(), out.sessionId(), out.url());
    }

    private static String normalizeCurrency(String raw) {
        final String v = (raw == null || raw.isBlank()) ? "brl" : raw.trim().toLowerCase();
        if (!v.matches("^[a-z]{3}$")) throw new IllegalArgumentException("billing: currency invalid");
        return v;
    }

    private static List<BillingDtos.LineItem> normalizeItems(List<BillingDtos.LineItem> items) {
        if (items.size() > 100) throw new IllegalArgumentException("billing: too many items (max 100)");

        return items.stream().map(i -> {
            if (i == null) throw new IllegalArgumentException("billing: item invalid");
            if (i.name() == null || i.name().isBlank()) throw new IllegalArgumentException("billing: item name required");
            if (i.name().length() > 200) throw new IllegalArgumentException("billing: item name too long (max 200 chars)");
            if (i.unitAmountCents() <= 0) throw new IllegalArgumentException("billing: unitAmountCents must be > 0");
            if (i.unitAmountCents() > 100_000_000L) throw new IllegalArgumentException("billing: unitAmountCents too large");
            if (i.quantity() <= 0) throw new IllegalArgumentException("billing: quantity must be > 0");
            if (i.quantity() > 10_000) throw new IllegalArgumentException("billing: quantity too large (max 10000)");
            return new BillingDtos.LineItem(i.name().trim(), i.unitAmountCents(), i.quantity());
        }).toList();
    }
}
