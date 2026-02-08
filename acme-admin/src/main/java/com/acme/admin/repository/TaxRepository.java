package com.acme.admin.repository;

import com.acme.admin.domain.TaxEntity;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;

import java.util.Collection;
import java.util.Optional;
import java.util.UUID;

public interface TaxRepository extends JpaRepository<TaxEntity, UUID> {

    @Query("select count(t.id) from TaxEntity t where t.id in :ids")
    long countByIdIn(@Param("ids") Collection<UUID> ids);

		boolean existsByCode(String code);

		Optional<TaxEntity> findByCode(String code);
}
