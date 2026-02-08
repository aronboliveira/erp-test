package com.acme.admin.validation;

import java.util.Collection;
import java.util.UUID;

public interface TaxIdsExistencePort {
    long countExisting(Collection<UUID> ids);
}
