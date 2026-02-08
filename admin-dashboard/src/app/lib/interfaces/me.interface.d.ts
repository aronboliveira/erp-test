export interface MeDto extends Readonly<{}> {
  id: string;
  username: string;
  roleNames: readonly string[];
  permissionCodes: readonly string[];
  createdAt?: string | null;
  lastLoginAt?: string | null;
}
