package com.acme.admin.domain;

import jakarta.persistence.*;
import org.hibernate.annotations.UuidGenerator;

import java.math.BigDecimal;
import java.util.UUID;

@Entity
@Table(name = "products_or_services")
public class ProductOrServiceEntity extends AuditedEntity {
  @Id
  @UuidGenerator
  @Column(columnDefinition = "uuid")
  private UUID id;

  @Enumerated(EnumType.STRING)
  @Column(nullable = false)
  private ProductKind kind;

  @Column(nullable = false, length = 180)
  private String name;

  @Column(length = 64)
  private String sku;

  @Column(nullable = false, precision = 15, scale = 2)
  private BigDecimal price;

  @Column(nullable = false, length = 3)
  private String currency;

  @ManyToOne(optional = false, fetch = FetchType.LAZY)
  @JoinColumn(name = "category_id", nullable = false)
  private ProductOrServiceCategoryEntity category;

  public UUID getId() { return id; }
  public ProductKind getKind() { return kind; }
  public void setKind(ProductKind kind) { this.kind = kind; }
  public String getName() { return name; }
  public void setName(String name) { this.name = name; }
  public String getSku() { return sku; }
  public void setSku(String sku) { this.sku = sku; }
  public BigDecimal getPrice() { return price; }
  public void setPrice(BigDecimal price) { this.price = price; }
  public String getCurrency() { return currency; }
  public void setCurrency(String currency) { this.currency = currency; }
  public ProductOrServiceCategoryEntity getCategory() { return category; }
  public void setCategory(ProductOrServiceCategoryEntity category) { this.category = category; }
}
