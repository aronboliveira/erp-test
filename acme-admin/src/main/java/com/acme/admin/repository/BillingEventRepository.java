package com.acme.admin.repository;

import com.acme.admin.domain.BillingEventEntity;
import org.springframework.data.domain.*;
import org.springframework.data.jpa.repository.JpaRepository;

import java.util.UUID;

public interface BillingEventRepository extends JpaRepository<BillingEventEntity, UUID> {
    boolean existsByEventId(String eventId);
		Page<BillingEventEntity> findAll(Pageable pageable);
}
