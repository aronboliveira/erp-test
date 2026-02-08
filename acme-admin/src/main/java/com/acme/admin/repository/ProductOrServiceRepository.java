package com.acme.admin.repository;

import com.acme.admin.domain.ProductOrServiceEntity;
import java.util.UUID;
import org.springframework.data.jpa.repository.JpaRepository;

public interface ProductOrServiceRepository extends JpaRepository<ProductOrServiceEntity, UUID> {}
