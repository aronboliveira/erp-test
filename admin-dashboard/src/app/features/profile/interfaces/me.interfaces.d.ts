export interface MeResponse {
  userId: string;
  email: string;
  displayName: string;
  roleNames: readonly string[];
  permissionCodes: readonly string[];
}
