package com.acme.admin.repository;

import com.acme.admin.domain.BudgetEntity;
import java.util.UUID;
import org.springframework.data.jpa.repository.JpaRepository;

public interface BudgetRepository extends JpaRepository<BudgetEntity, UUID> {}
