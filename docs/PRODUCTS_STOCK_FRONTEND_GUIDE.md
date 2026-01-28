# Products & Stock Management - Frontend Integration Guide

## 📋 Índice
1. [Products API](#products-api)
2. [Stock API](#stock-api)
3. [TypeScript Models](#typescript-models)
4. [Angular Services](#angular-services)
5. [Examples](#examples)

---

## 📦 **PRODUCTS API**

### **Base URL**
```
https://localhost:7001/api/products
```

### **Endpoints**

#### 1. Search Products (with Pagination) ⭐ **NEW**
```http
GET /api/products/search?searchTerm=notebook&page=1&pageSize=20
Authorization: Bearer {token}

Query Parameters:
- searchTerm (optional): Search in name, SKU, description, barcode
- categoryId (optional): Filter by category GUID
- supplierId (optional): Filter by supplier GUID
- status (optional): "Active" | "Inactive" | "Discontinued"
- minPrice (optional): Minimum price filter
- maxPrice (optional): Maximum price filter
- page (default: 1): Page number
- pageSize (default: 20): Items per page
- sortBy (default: "Name"): "Name" | "Sku" | "Price" | "CreatedAt"
- sortOrder (default: "asc"): "asc" | "desc"

Response 200:
{
  "items": [
    {
      "id": "guid",
      "name": "Notebook Dell Inspiron",
      "description": "15.6 inch, 8GB RAM, 256GB SSD",
      "sku": "NB-DELL-001",
      "barcode": "7891234567890",
      "priceAmount": 3500.00,
      "priceCurrency": "BRL",
      "costPriceAmount": 2800.00,
      "costPriceCurrency": "BRL",
      "status": "Active",
      "categoryId": "guid",
      "categoryName": "Notebooks",
      "supplierId": "guid",
      "supplierName": "Dell Brasil",
      "minimumStock": 5,
      "imageUrl": "https://...",
      "createdAt": "2024-01-26T10:00:00Z",
      "updatedAt": null
    }
  ],
  "totalCount": 150,
  "page": 1,
  "pageSize": 20,
  "totalPages": 8,
  "hasPrevious": false,
  "hasNext": true
}

Permissions: Admin, Manager, Operator, Viewer
```

#### 2. Get All Products (Simple List)
```http
GET /api/products
Authorization: Bearer {token}

Response 200: Array of ProductDto

Permissions: Admin, Manager, Operator, Viewer
```

#### 3. Get Product by ID
```http
GET /api/products/{id}
Authorization: Bearer {token}

Response 200: ProductDto
Response 404: { "error": "Product not found" }

Permissions: Admin, Manager, Operator, Viewer
```

#### 4. Get Product by SKU
```http
GET /api/products/sku/{sku}
Authorization: Bearer {token}

Response 200: ProductDto
Response 404: { "error": "Product not found" }

Permissions: Admin, Manager, Operator, Viewer
```

#### 5. Create Product
```http
POST /api/products
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "Mouse Logitech MX Master 3",
  "description": "Wireless ergonomic mouse",
  "sku": "MS-LOG-MX3",
  "barcode": "7891234567891",
  "priceAmount": 499.90,
  "priceCurrency": "BRL",
  "costPriceAmount": 350.00,
  "costPriceCurrency": "BRL",
  "categoryId": "guid-categoria",
  "supplierId": "guid-fornecedor",
  "minimumStock": 10,
  "imageUrl": "https://example.com/image.jpg"
}

Response 201: { "id": "guid-produto-criado" }

Validations:
- name: required, max 200 chars
- sku: required, max 50 chars, uppercase letters/numbers/hyphens only, UNIQUE
- priceAmount >= 0
- costPriceAmount >= 0
- priceAmount >= costPriceAmount (sale price >= cost price)
- priceCurrency: required, 3 chars (BRL, USD)
- categoryId: required, must exist
- supplierId: optional, must exist if provided
- minimumStock >= 0
- barcode: optional, max 50 chars
- imageUrl: optional, max 500 chars, valid URL

Errors:
- 400: "Product with SKU 'MS-LOG-MX3' already exists"
- 400: "Category not found"
- 400: "Sale price must be greater than or equal to cost price"
- 400: "SKU must contain only uppercase letters, numbers, and hyphens"

Permissions: Admin, Manager
```

#### 6. Update Product
```http
PUT /api/products/{id}
Authorization: Bearer {token}
Content-Type: application/json

{
  "id": "guid-produto",
  "name": "Mouse Logitech MX Master 3S",
  "description": "Updated description",
  "priceAmount": 549.90,
  "priceCurrency": "BRL",
  "costPriceAmount": 380.00,
  "costPriceCurrency": "BRL",
  "categoryId": "guid",
  "supplierId": "guid",
  "minimumStock": 15,
  "barcode": "7891234567891",
  "imageUrl": "https://..."
}

Response 204: No Content
Response 400: { "error": "ID mismatch" }
Response 404: { "error": "Product not found" }

Permissions: Admin, Manager
```

#### 7. Delete Product (Soft Delete) ⭐ **NEW**
```http
DELETE /api/products/{id}
Authorization: Bearer {token}

Response 204: No Content
Response 400: { "error": "Cannot delete product with existing stock. Remove stock first." }
Response 404: { "error": "Product not found" }

Note: This is a SOFT DELETE. The product is marked as Inactive but not removed from database.

Permissions: Admin, Manager
```

---

## 📊 **STOCK API**

### **Base URL**
```
https://localhost:7001/api/stock
```

### **Endpoints**

#### 1. Get Stock by Product
```http
GET /api/stock/product/{productId}
Authorization: Bearer {token}

Response 200:
{
  "id": "guid",
  "productId": "guid",
  "productName": "Notebook Dell",
  "productSku": "NB-DELL-001",
  "quantity": 25,
  "reservedQuantity": 3,
  "availableQuantity": 22,
  "minimumStock": 5,
  "location": "Warehouse A - Shelf 12",
  "isLowStock": false,
  "createdAt": "2024-01-26T10:00:00Z",
  "updatedAt": "2024-01-26T15:30:00Z"
}

Response 404: { "error": "Stock item not found for this product" }

Permissions: Admin, Manager, Operator, Viewer
```

#### 2. Get Low Stock Products
```http
GET /api/stock/low-stock
Authorization: Bearer {token}

Response 200: Array of StockItemDto (where quantity <= minimumStock)

Permissions: Admin, Manager, Operator
```

#### 3. Add Stock (Entrada)
```http
POST /api/stock/add
Authorization: Bearer {token}
Content-Type: application/json

{
  "productId": "guid",
  "quantity": 50,
  "reference": "Invoice #12345",
  "notes": "Purchase from supplier XYZ"
}

Response 200:
{
  "success": true,
  "message": "Stock added successfully"
}

Validations:
- productId: required, must exist
- quantity: required, must be > 0
- reference: optional, max 100 chars
- notes: optional, max 500 chars

Errors:
- 400: "Product not found"
- 400: "Quantity must be greater than zero"

Permissions: Admin, Manager, Operator
```

#### 4. Remove Stock (Saída)
```http
POST /api/stock/remove
Authorization: Bearer {token}
Content-Type: application/json

{
  "productId": "guid",
  "quantity": 5,
  "reference": "Sale #98765",
  "notes": "Sale to customer ABC"
}

Response 200:
{
  "success": true,
  "message": "Stock removed successfully"
}

Validations:
- quantity <= availableQuantity (cannot remove more than available)
- Cannot result in negative stock

Errors:
- 400: "Insufficient stock"
- 400: "Product not found"

Permissions: Admin, Manager, Operator
```

#### 5. Adjust Stock (Ajuste/Inventário)
```http
POST /api/stock/adjust
Authorization: Bearer {token}
Content-Type: application/json

{
  "productId": "guid",
  "newQuantity": 100,
  "reference": "Inventory 2024-01",
  "notes": "Monthly inventory adjustment"
}

Response 200:
{
  "success": true,
  "message": "Stock adjusted successfully"
}

Validations:
- newQuantity >= reservedQuantity (cannot adjust below reserved)
- newQuantity >= 0

Errors:
- 400: "Product not found"
- 400: "Cannot adjust stock below reserved quantity"

Permissions: Admin, Manager
```

---

## 🎨 **TypeScript Models**

### **Products**
```typescript
// product.models.ts

export enum ProductStatus {
  Active = 'Active',
  Inactive = 'Inactive',
  Discontinued = 'Discontinued'
}

export interface Product {
  id: string;
  name: string;
  description: string;
  sku: string;
  barcode?: string;
  priceAmount: number;
  priceCurrency: string;
  costPriceAmount: number;
  costPriceCurrency: string;
  status: ProductStatus;
  categoryId: string;
  categoryName?: string;
  supplierId?: string;
  supplierName?: string;
  minimumStock: number;
  imageUrl?: string;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateProductRequest {
  name: string;
  description: string;
  sku: string;
  priceAmount: number;
  priceCurrency: string;
  costPriceAmount: number;
  costPriceCurrency: string;
  categoryId: string;
  supplierId?: string;
  minimumStock: number;
  barcode?: string;
  imageUrl?: string;
}

export interface UpdateProductRequest extends CreateProductRequest {
  id: string;
}

export interface ProductSearchParams {
  searchTerm?: string;
  categoryId?: string;
  supplierId?: string;
  status?: ProductStatus;
  minPrice?: number;
  maxPrice?: number;
  page?: number;
  pageSize?: number;
  sortBy?: 'Name' | 'Sku' | 'Price' | 'CreatedAt';
  sortOrder?: 'asc' | 'desc';
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}
```

### **Stock**
```typescript
// stock.models.ts

export interface StockItem {
  id: string;
  productId: string;
  productName: string;
  productSku: string;
  quantity: number;
  reservedQuantity: number;
  availableQuantity: number;
  minimumStock: number;
  location?: string;
  isLowStock: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface AddStockRequest {
  productId: string;
  quantity: number;
  reference?: string;
  notes?: string;
}

export interface RemoveStockRequest {
  productId: string;
  quantity: number;
  reference?: string;
  notes?: string;
}

export interface AdjustStockRequest {
  productId: string;
  newQuantity: number;
  reference?: string;
  notes?: string;
}

export interface StockOperationResponse {
  success: boolean;
  message: string;
}
```

---

## 🎯 **Validation Rules Summary**

### **Products**
| Field | Required | Type | Validation |
|-------|----------|------|------------|
| name | ✅ | string | max 200 chars |
| description | ✅ | string | - |
| sku | ✅ | string | max 50, unique, uppercase/numbers/hyphens |
| priceAmount | ✅ | number | >= 0, >= costPriceAmount |
| priceCurrency | ✅ | string | exactly 3 chars |
| costPriceAmount | ✅ | number | >= 0 |
| categoryId | ✅ | GUID | must exist |
| supplierId | ❌ | GUID | must exist if provided |
| minimumStock | ✅ | number | >= 0 |
| barcode | ❌ | string | max 50 chars |
| imageUrl | ❌ | string | max 500 chars, valid URL |

### **Stock Operations**
| Field | Required | Type | Validation |
|-------|----------|------|------------|
| productId | ✅ | GUID | must exist |
| quantity (add/remove) | ✅ | number | > 0 |
| newQuantity (adjust) | ✅ | number | >= 0, >= reservedQuantity |
| reference | ❌ | string | max 100 chars |
| notes | ❌ | string | max 500 chars |

---

## ✅ **Testing Checklist**

- [ ] Search products with different filters
- [ ] Pagination (next/previous pages)
- [ ] Create product with valid data
- [ ] Create product with duplicate SKU (should fail)
- [ ] Create product with price < cost (should fail)
- [ ] Update product
- [ ] Delete product without stock
- [ ] Delete product with stock (should fail)
- [ ] View stock by product
- [ ] Add stock with valid quantity
- [ ] Remove stock (valid quantity)
- [ ] Remove stock (more than available - should fail)
- [ ] Adjust stock
- [ ] View low stock products

---

**🎉 Ready to integrate with Angular!**
