package com.acme.admin.controller.security;

import com.acme.admin.dto.security.UserDtos.ProfileDto;
import com.acme.admin.service.security.UserService;
import org.springframework.security.access.prepost.PreAuthorize;
import org.springframework.security.core.Authentication;
import org.springframework.web.bind.annotation.*;

@RestController
@RequestMapping("/api/me")
public class MeController {
  private final UserService users;

  public MeController(UserService users) {
    this.users = users;
  }

  @GetMapping
  @PreAuthorize("isAuthenticated()")
  public ProfileDto profile(Authentication auth) {
    return users.profileByUsername(auth.getName());
  }
}
