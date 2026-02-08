import { ObjectMapper } from '../../shared/utils/object-mapper.adapter';

export const ApiRoutes = ObjectMapper.deepFreeze({
  billing: {
    checkoutSession: '/api/billing/checkout-session',
  },
  me: '/api/me',
} as const);
