package com.acme.admin.service;

import com.acme.admin.dto.MeDto;

public interface UserProfileService {
    MeDto getMe(String username);
}
