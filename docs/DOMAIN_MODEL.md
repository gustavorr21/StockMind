# StockMind - Domain Model Documentation

## Overview
This document describes the domain model for StockMind, an inventory management system built with Clean Architecture and Domain-Driven Design principles.

## Value Objects

### Money
Represents monetary values with currency.
- **Properties**: `Amount` (decimal), `Currency` (string)
- **Operations**: Add, Subtract, Multiply
- **Validation**: Amount cannot be negative, Currency cannot be empty
- **Immutable**: Yes

### Email
Represents an email address.
- **Properties**: `Value` (string)
- **Validation**: Must match email regex pattern
- **Immutable**: Yes

### Phone
Represents a phone number.
- **Properties**: `Number` (string)
- **Validation**: Must be 10-13 digits after cleaning
- **Immutable**: Yes

### Address
Represents a physical address.
- **Properties**: `Street`, `Number`, `Complement`, `City`, `State`, `ZipCode`, `Country`
- **Validation**: Required fields cannot be empty
- **Immutable**: Yes

---

## Enums

### StockMovementType
- `Purchase` = 1
- `Sale` = 2
- `Return` = 3
- `Adjustment` = 4
- `Transfer` = 5
- `Loss` = 6
- `Damage` = 7

### ProductStatus
- `Active` = 1
- `Inactive` = 2
- `Discontinued` = 3
- `OutOfStock` = 4

### SupplierStatus
- `Active` = 1
- `Inactive` = 2
- `Blocked` = 3

---

## Domain Events

### ProductCreatedEvent
Raised when a new product is created.
- **Properties**: `ProductId`, `ProductName`, `Sku`, `OccurredOn`

### StockUpdatedEvent
Raised when stock quantity changes.
- **Properties**: `ProductId`, `PreviousQuantity`, `NewQuantity`, `MovementType`, `OccurredOn`

### LowStockEvent
Raised when stock falls below minimum threshold.
- **Properties**: `ProductId`, `ProductName`, `CurrentQuantity`, `MinimumQuantity`, `OccurredOn`

---

## Entities

### Category (BaseEntity)
Represents a product category.

**Properties:**
- `Name` (string, max 100 chars) - Required
- `Description` (string)
- `IsActive` (bool)

**Business Rules:**
- Name cannot be empty
- Name cannot exceed 100 characters

**Operations:**
- `Create()` - Factory method
- `UpdateName()` - Update category name
- `UpdateDescription()` - Update description
- `Activate()` - Activate category
- `Deactivate()` - Deactivate category

---

### Supplier (BaseEntity)
Represents a supplier/vendor.

**Properties:**
- `CompanyName` (string) - Required
- `TradeName` (string) - Required
- `Document` (string) - Required (CNPJ/Tax ID)
- `Email` (Email VO) - Required
- `Phone` (Phone VO) - Required
- `Address` (Address VO) - Required
- `Status` (SupplierStatus)
- `Notes` (string, nullable)

**Business Rules:**
- Company name, trade name, and document are required
- Contact information must be valid
- Status can be Active, Inactive, or Blocked

**Operations:**
- `Create()` - Factory method
- `UpdateContactInfo()` - Update email and phone
- `UpdateAddress()` - Update address
- `UpdateStatus()` - Change status
- `AddNotes()` - Add or update notes
- `Activate()` - Set status to Active
- `Deactivate()` - Set status to Inactive
- `Block()` - Set status to Blocked

---

### Product (AggregateRoot)
Represents a product in the inventory.

**Properties:**
- `Name` (string, max 200 chars) - Required
- `Description` (string)
- `Sku` (string, max 50 chars) - Required, Uppercase
- `Barcode` (string, max 50 chars, nullable)
- `Price` (Money VO) - Required
- `CostPrice` (Money VO) - Required
- `Status` (ProductStatus)
- `CategoryId` (Guid) - Required
- `SupplierId` (Guid, nullable)
- `MinimumStock` (int)
- `ImageUrl` (string, nullable)

**Relationships:**
- Belongs to one `Category`
- May have one `Supplier`

**Business Rules:**
- Name and SKU are required
- SKU is converted to uppercase
- Price and cost price must be valid Money objects
- Minimum stock cannot be negative
- Raises `ProductCreatedEvent` on creation

**Operations:**
- `Create()` - Factory method
- `UpdateBasicInfo()` - Update name and description
- `UpdatePrice()` - Update selling price
- `UpdateCostPrice()` - Update cost price
- `SetBarcode()` - Set or update barcode
- `AssignSupplier()` - Link to supplier
- `RemoveSupplier()` - Remove supplier link
- `UpdateCategory()` - Change category
- `UpdateMinimumStock()` - Update minimum stock level
- `SetImageUrl()` - Set product image
- `Activate()` - Set status to Active
- `Deactivate()` - Set status to Inactive
- `MarkAsDiscontinued()` - Mark as discontinued
- `MarkAsOutOfStock()` - Mark as out of stock

---

### StockItem (AggregateRoot)
Represents the current stock status of a product.

**Properties:**
- `ProductId` (Guid) - Required
- `Quantity` (int) - Total quantity in stock
- `ReservedQuantity` (int) - Quantity reserved for orders
- `AvailableQuantity` (computed) - Quantity - ReservedQuantity
- `Location` (string, nullable) - Storage location

**Relationships:**
- One-to-one with `Product`

**Business Rules:**
- Quantity cannot be negative
- Cannot remove more than available quantity
- Cannot adjust stock below reserved quantity
- Raises `StockUpdatedEvent` on quantity changes
- Raises `LowStockEvent` when stock falls below minimum

**Operations:**
- `Create()` - Factory method
- `AddStock()` - Increase stock quantity
- `RemoveStock()` - Decrease stock quantity
- `AdjustStock()` - Set stock to specific quantity
- `ReserveStock()` - Reserve quantity for orders
- `ReleaseReservedStock()` - Release reserved quantity
- `UpdateLocation()` - Update storage location

---

### StockMovement (BaseEntity)
Audit trail for stock changes.

**Properties:**
- `ProductId` (Guid) - Required
- `MovementType` (StockMovementType) - Required
- `Quantity` (int) - Required, positive
- `PreviousQuantity` (int) - Stock before movement
- `NewQuantity` (int) - Stock after movement
- `Reference` (string, nullable) - External reference (order ID, etc.)
- `Notes` (string, nullable)
- `UserId` (Guid, nullable) - Who performed the movement

**Relationships:**
- Many-to-one with `Product`

**Business Rules:**
- Quantity must be positive
- Previous and new quantities cannot be negative
- Immutable once created (except notes)

**Operations:**
- `Create()` - Factory method
- `AddNotes()` - Add or update notes

---

## Aggregate Roots

The domain has **2 aggregate roots**:

1. **Product** - Controls product information and lifecycle
2. **StockItem** - Controls stock operations and inventory levels

## Domain Invariants

1. **Stock Consistency**: Stock quantity must always match the sum of all movements
2. **Reserved Stock**: Reserved quantity cannot exceed total quantity
3. **Low Stock Alert**: System must notify when stock falls below minimum
4. **Product Lifecycle**: Products must have valid category and pricing
5. **Supplier Validation**: Suppliers must have complete contact information

## Event-Driven Architecture

Domain events enable:
- **Audit trail** - Track all stock changes
- **Notifications** - Alert on low stock
- **Integration** - Trigger external systems (RabbitMQ)
- **AI Analysis** - Feed data to AI prediction models

## Next Steps

1. ? Value Objects created
2. ? Enums defined
3. ? Domain events implemented
4. ? Entities with rich domain logic
5. ? Repository interfaces (next)
6. ? Application use cases (next)
7. ? API endpoints (next)
8. ? Database configurations (next)

---

## Design Patterns Used

- **Value Object** - Immutable domain concepts
- **Aggregate Root** - Consistency boundaries
- **Domain Events** - Decouple domain logic
- **Factory Method** - Controlled entity creation
- **Guard Clauses** - Validation in constructors
- **Rich Domain Model** - Business logic in entities (not anemic)
