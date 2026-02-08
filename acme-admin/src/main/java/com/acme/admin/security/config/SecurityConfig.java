package com.acme.admin.security.config;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.context.annotation.*;
import org.springframework.security.authentication.dao.DaoAuthenticationProvider;
import org.springframework.security.config.annotation.method.configuration.EnableMethodSecurity;
import org.springframework.security.config.annotation.web.builders.HttpSecurity;
import org.springframework.security.config.http.SessionCreationPolicy;
import org.springframework.security.crypto.bcrypt.BCryptPasswordEncoder;
import org.springframework.security.crypto.password.PasswordEncoder;
import org.springframework.security.web.SecurityFilterChain;
import org.springframework.security.web.authentication.UsernamePasswordAuthenticationFilter;

@Configuration
@EnableMethodSecurity
public class SecurityConfig {

  @Autowired(required = false)
  private MockUserAuthFilter mockAuth;

  @Bean
  PasswordEncoder passwordEncoder() {
    return new BCryptPasswordEncoder();
  }

  @Bean
  DaoAuthenticationProvider authProvider(AuthUserDetailsService uds, PasswordEncoder encoder) {
    final DaoAuthenticationProvider p = new DaoAuthenticationProvider();
    p.setUserDetailsService(uds);
    p.setPasswordEncoder(encoder);
    return p;
  }

  @Bean
  SecurityFilterChain filterChain(HttpSecurity http) throws Exception {
    http
      .csrf(csrf -> csrf.disable())
      .sessionManagement(sm -> sm.sessionCreationPolicy(SessionCreationPolicy.STATELESS))
      .headers(headers -> headers
        .frameOptions(frame -> frame.deny())
        .contentSecurityPolicy(csp -> csp.policyDirectives("default-src 'self'"))
      )
      .authorizeHttpRequests(reg -> reg
        .requestMatchers("/api/**").authenticated()
        .requestMatchers("/actuator/health").permitAll()
        .anyRequest().permitAll()
      )
      .httpBasic(b -> {});

    if (mockAuth != null) {
      http.addFilterBefore(mockAuth, UsernamePasswordAuthenticationFilter.class);
    }

    return http.build();
  }
}
