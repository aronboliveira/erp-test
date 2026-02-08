package com.acme.admin.repository;

import com.acme.admin.domain.*;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.UUID;

public interface PurchaseRepository extends JpaRepository<PurchaseEntity, UUID> {}
