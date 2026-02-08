export interface CheckoutSessionRequest {
  currency: string;
  customerEmail: string | null;
  items: readonly CheckoutLineItem[];
  successUrl?: string | null;
  cancelUrl?: string | null;
}

export interface CheckoutLineItem {
  name: string;
  unitAmountCents: number;
  quantity: number;
}

export interface CheckoutSessionResponse {
  provider: string;
  sessionId: string;
  url: string;
}
