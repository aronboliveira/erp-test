package com.acme.admin.validation;

import java.util.Collection;

public interface IdsExistenceValidator<ID> {
  ValidationResult validate(String field, Collection<ID> ids, ValidationResult acc);
}
