package com.acme.admin.repository;

import com.acme.admin.domain.RevenueEntity;
import java.util.UUID;
import org.springframework.data.jpa.repository.JpaRepository;

public interface RevenueRepository extends JpaRepository<RevenueEntity, UUID> {}
