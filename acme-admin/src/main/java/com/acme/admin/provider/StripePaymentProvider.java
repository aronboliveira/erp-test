package com.acme.admin.provider;

import com.acme.admin.domain.BillingProperties;
import com.stripe.Stripe;
import com.stripe.model.checkout.Session;
import com.stripe.param.checkout.SessionCreateParams;

import org.springframework.stereotype.Component;

@Component
public final class StripePaymentProvider implements PaymentProvider {

    private static final String PROVIDER = "stripe";
    private final BillingProperties props;

    public StripePaymentProvider(BillingProperties props) {
        this.props = props;
        if (props.secretKey() != null && !props.secretKey().isBlank())
            Stripe.apiKey = props.secretKey();
    }

    @Override
    public CheckoutSessionResult createCheckoutSession(CreateCheckoutSessionCommand cmd) throws Exception {
        final SessionCreateParams.Builder b = SessionCreateParams.builder()
            .setMode(SessionCreateParams.Mode.PAYMENT)
            .setSuccessUrl(cmd.successUrl())
            .setCancelUrl(cmd.cancelUrl());

        if (cmd.customerEmail() != null && !cmd.customerEmail().isBlank())
            b.setCustomerEmail(cmd.customerEmail());

        for (final LineItem i : cmd.items()) {
            final SessionCreateParams.LineItem.PriceData.ProductData pd =
                SessionCreateParams.LineItem.PriceData.ProductData.builder()
                    .setName(i.name())
                    .build();

            final SessionCreateParams.LineItem.PriceData price =
                SessionCreateParams.LineItem.PriceData.builder()
                    .setCurrency(cmd.currency())
                    .setUnitAmount(i.unitAmountCents())
                    .setProductData(pd)
                    .build();

            final SessionCreateParams.LineItem li =
                SessionCreateParams.LineItem.builder()
                    .setPriceData(price)
                    .setQuantity(i.quantity())
                    .build();

            b.addLineItem(li);
        }

        final Session s = Session.create(b.build());
        return new CheckoutSessionResult(PROVIDER, s.getId(), s.getUrl());
    }
}
