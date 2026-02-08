package com.acme.admin.repository;

import com.acme.admin.domain.ExpenseEntity;
import java.util.UUID;
import org.springframework.data.jpa.repository.JpaRepository;

public interface ExpenseRepository extends JpaRepository<ExpenseEntity, UUID> {}
