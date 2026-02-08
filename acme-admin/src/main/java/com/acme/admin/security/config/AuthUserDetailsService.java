package com.acme.admin.security.config;

import com.acme.admin.domain.security.*;
import com.acme.admin.repository.security.AuthUserRepository;
import org.springframework.security.core.*;
import org.springframework.security.core.authority.SimpleGrantedAuthority;
import org.springframework.security.core.userdetails.*;
import org.springframework.stereotype.Service;

import java.util.*;

@Service
public class AuthUserDetailsService implements UserDetailsService {
  private final AuthUserRepository users;

  public AuthUserDetailsService(AuthUserRepository users) {
    this.users = users;
  }

  @Override
  public UserDetails loadUserByUsername(String username) throws UsernameNotFoundException {
    final AuthUser u = users.findByUsername(username)
      .orElseThrow(() -> new UsernameNotFoundException("user not found"));

    if (u.getStatus() != AuthUser.Status.ACTIVE)
      throw new UsernameNotFoundException("user not active");

    final Set<String> permCodes = new LinkedHashSet<>();
    for (final AuthRole r : u.getRoles())
      for (final AuthPermission p : r.getPermissions())
        permCodes.add(p.getCode());

    final List<GrantedAuthority> auths = permCodes.stream()
      .<GrantedAuthority>map(SimpleGrantedAuthority::new)
      .toList();

    return org.springframework.security.core.userdetails.User
      .withUsername(u.getUsername())
      .password(u.getPasswordHash())
      .authorities(auths)
      .build();
  }
}
