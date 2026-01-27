# Products & Stock Management - Frontend Integration Guide

## ?? Índice
1. [Products API](#products-api)
2. [Stock API](#stock-api)
3. [TypeScript Models](#typescript-models)
4. [Angular Services](#angular-services)
5. [Examples](#examples)

---

## ?? **PRODUCTS API**

### **Base URL**
```
https://localhost:7001/api/products
```

### **Endpoints**

#### 1. Search Products (with Pagination) ? **NEW**
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

#### 7. Delete Product (Soft Delete) ? **NEW**
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

## ?? **STOCK API**

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

## ?? **TypeScript Models**

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

## ?? **Angular Services**

### **Product Service**
```typescript
// product.service.ts
import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '@environments/environment';
import {
  Product,
  CreateProductRequest,
  UpdateProductRequest,
  ProductSearchParams,
  PagedResult
} from '@core/models/product.models';

@Injectable({ providedIn: 'root' })
export class ProductService {
  private apiUrl = `${environment.apiUrl}/products`;

  constructor(private http: HttpClient) {}

  search(params: ProductSearchParams): Observable<PagedResult<Product>> {
    let httpParams = new HttpParams();

    if (params.searchTerm) httpParams = httpParams.set('searchTerm', params.searchTerm);
    if (params.categoryId) httpParams = httpParams.set('categoryId', params.categoryId);
    if (params.supplierId) httpParams = httpParams.set('supplierId', params.supplierId);
    if (params.status) httpParams = httpParams.set('status', params.status);
    if (params.minPrice) httpParams = httpParams.set('minPrice', params.minPrice.toString());
    if (params.maxPrice) httpParams = httpParams.set('maxPrice', params.maxPrice.toString());
    if (params.page) httpParams = httpParams.set('page', params.page.toString());
    if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());
    if (params.sortBy) httpParams = httpParams.set('sortBy', params.sortBy);
    if (params.sortOrder) httpParams = httpParams.set('sortOrder', params.sortOrder);

    return this.http.get<PagedResult<Product>>(`${this.apiUrl}/search`, { params: httpParams });
  }

  getAll(): Observable<Product[]> {
    return this.http.get<Product[]>(this.apiUrl);
  }

  getById(id: string): Observable<Product> {
    return this.http.get<Product>(`${this.apiUrl}/${id}`);
  }

  getBySku(sku: string): Observable<Product> {
    return this.http.get<Product>(`${this.apiUrl}/sku/${sku}`);
  }

  create(request: CreateProductRequest): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(this.apiUrl, request);
  }

  update(id: string, request: UpdateProductRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
```

### **Stock Service**
```typescript
// stock.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '@environments/environment';
import {
  StockItem,
  AddStockRequest,
  RemoveStockRequest,
  AdjustStockRequest,
  StockOperationResponse
} from '@core/models/stock.models';

@Injectable({ providedIn: 'root' })
export class StockService {
  private apiUrl = `${environment.apiUrl}/stock`;

  constructor(private http: HttpClient) {}

  getByProductId(productId: string): Observable<StockItem> {
    return this.http.get<StockItem>(`${this.apiUrl}/product/${productId}`);
  }

  getLowStock(): Observable<StockItem[]> {
    return this.http.get<StockItem[]>(`${this.apiUrl}/low-stock`);
  }

  addStock(request: AddStockRequest): Observable<StockOperationResponse> {
    return this.http.post<StockOperationResponse>(`${this.apiUrl}/add`, request);
  }

  removeStock(request: RemoveStockRequest): Observable<StockOperationResponse> {
    return this.http.post<StockOperationResponse>(`${this.apiUrl}/remove`, request);
  }

  adjustStock(request: AdjustStockRequest): Observable<StockOperationResponse> {
    return this.http.post<StockOperationResponse>(`${this.apiUrl}/adjust`, request);
  }
}
```

---

## ?? **Examples**

### **1. Search Products with Pagination**
```typescript
// product-list.component.ts
export class ProductListComponent implements OnInit {
  products: Product[] = [];
  totalCount = 0;
  page = 1;
  pageSize = 20;
  searchTerm = '';

  constructor(private productService: ProductService) {}

  ngOnInit(): void {
    this.loadProducts();
  }

  loadProducts(): void {
    this.productService.search({
      searchTerm: this.searchTerm,
      page: this.page,
      pageSize: this.pageSize,
      sortBy: 'Name',
      sortOrder: 'asc'
    }).subscribe({
      next: (result) => {
        this.products = result.items;
        this.totalCount = result.totalCount;
      },
      error: (error) => console.error('Error loading products', error)
    });
  }

  onSearch(term: string): void {
    this.searchTerm = term;
    this.page = 1;
    this.loadProducts();
  }

  onPageChange(page: number): void {
    this.page = page;
    this.loadProducts();
  }
}
```

### **2. Create Product**
```typescript
// product-form.component.ts
export class ProductFormComponent {
  productForm: FormGroup;

  constructor(
    private fb: FormBuilder,
    private productService: ProductService,
    private router: Router
  ) {
    this.productForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(200)]],
      description: ['', Validators.required],
      sku: ['', [Validators.required, Validators.pattern(/^[A-Z0-9-]+$/)]],
      priceAmount: [0, [Validators.required, Validators.min(0)]],
      priceCurrency: ['BRL', [Validators.required, Validators.minLength(3), Validators.maxLength(3)]],
      costPriceAmount: [0, [Validators.required, Validators.min(0)]],
      costPriceCurrency: ['BRL', [Validators.required, Validators.minLength(3), Validators.maxLength(3)]],
      categoryId: ['', Validators.required],
      supplierId: [''],
      minimumStock: [0, [Validators.required, Validators.min(0)]],
      barcode: [''],
      imageUrl: ['']
    });
  }

  onSubmit(): void {
    if (this.productForm.valid) {
      const request: CreateProductRequest = this.productForm.value;
      
      this.productService.create(request).subscribe({
        next: (response) => {
          console.log('Product created:', response.id);
          this.router.navigate(['/products', response.id]);
        },
        error: (error) => {
          console.error('Error creating product:', error.error.error);
        }
      });
    }
  }
}
```

### **3. Delete Product**
```typescript
// product-detail.component.ts
export class ProductDetailComponent {
  product: Product;

  constructor(
    private productService: ProductService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar,
    private router: Router
  ) {}

  onDelete(): void {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Delete Product',
        message: 'Are you sure you want to delete this product? This action cannot be undone.'
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.productService.delete(this.product.id).subscribe({
          next: () => {
            this.snackBar.open('Product deleted successfully', 'Close', { duration: 3000 });
            this.router.navigate(['/products']);
          },
          error: (error) => {
            this.snackBar.open(error.error.error, 'Close', { duration: 5000 });
          }
        });
      }
    });
  }
}
```

### **4. Add Stock**
```typescript
// stock-add.component.ts
export class StockAddComponent {
  addStockForm: FormGroup;

  constructor(
    private fb: FormBuilder,
    private stockService: StockService,
    private snackBar: MatSnackBar
  ) {
    this.addStockForm = this.fb.group({
      productId: ['', Validators.required],
      quantity: [0, [Validators.required, Validators.min(1)]],
      reference: [''],
      notes: ['']
    });
  }

  onSubmit(): void {
    if (this.addStockForm.valid) {
      const request: AddStockRequest = this.addStockForm.value;
      
      this.stockService.addStock(request).subscribe({
        next: (response) => {
          this.snackBar.open(response.message, 'Close', { duration: 3000 });
          this.addStockForm.reset();
        },
        error: (error) => {
          this.snackBar.open(error.error.error, 'Close', { duration: 5000 });
        }
      });
    }
  }
}
```

### **5. Remove Stock**
```typescript
// stock-remove.component.ts
export class StockRemoveComponent {
  removeStockForm: FormGroup;
  currentStock: StockItem;

  constructor(
    private fb: FormBuilder,
    private stockService: StockService,
    private snackBar: MatSnackBar
  ) {
    this.removeStockForm = this.fb.group({
      productId: ['', Validators.required],
      quantity: [0, [Validators.required, Validators.min(1)]],
      reference: [''],
      notes: ['']
    });
  }

  onProductSelect(productId: string): void {
    this.stockService.getByProductId(productId).subscribe({
      next: (stock) => {
        this.currentStock = stock;
        // Update quantity validator to max available
        this.removeStockForm.get('quantity')?.setValidators([
          Validators.required,
          Validators.min(1),
          Validators.max(stock.availableQuantity)
        ]);
      }
    });
  }

  onSubmit(): void {
    if (this.removeStockForm.valid) {
      const request: RemoveStockRequest = this.removeStockForm.value;
      
      this.stockService.removeStock(request).subscribe({
        next: (response) => {
          this.snackBar.open(response.message, 'Close', { duration: 3000 });
          this.removeStockForm.reset();
        },
        error: (error) => {
          this.snackBar.open(error.error.error, 'Close', { duration: 5000 });
        }
      });
    }
  }
}
```

### **6. Low Stock Alert**
```typescript
// low-stock.component.ts
export class LowStockComponent implements OnInit {
  lowStockItems: StockItem[] = [];
  loading = true;

  constructor(private stockService: StockService) {}

  ngOnInit(): void {
    this.loadLowStock();
  }

  loadLowStock(): void {
    this.stockService.getLowStock().subscribe({
      next: (items) => {
        this.lowStockItems = items;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading low stock', error);
        this.loading = false;
      }
    });
  }
}
```

---

## ?? **Validation Rules Summary**

### **Products**
| Field | Required | Type | Validation |
|-------|----------|------|------------|
| name | ? | string | max 200 chars |
| description | ? | string | - |
| sku | ? | string | max 50, unique, uppercase/numbers/hyphens |
| priceAmount | ? | number | >= 0, >= costPriceAmount |
| priceCurrency | ? | string | exactly 3 chars |
| costPriceAmount | ? | number | >= 0 |
| categoryId | ? | GUID | must exist |
| supplierId | ? | GUID | must exist if provided |
| minimumStock | ? | number | >= 0 |
| barcode | ? | string | max 50 chars |
| imageUrl | ? | string | max 500 chars, valid URL |

### **Stock Operations**
| Field | Required | Type | Validation |
|-------|----------|------|------------|
| productId | ? | GUID | must exist |
| quantity (add/remove) | ? | number | > 0 |
| newQuantity (adjust) | ? | number | >= 0, >= reservedQuantity |
| reference | ? | string | max 100 chars |
| notes | ? | string | max 500 chars |

---

## ?? **Permissions Summary**

| Action | Admin | Manager | Operator | Viewer |
|--------|-------|---------|----------|--------|
| **Products** |
| View/Search | ? | ? | ? | ? |
| Create | ? | ? | ? | ? |
| Update | ? | ? | ? | ? |
| Delete | ? | ? | ? | ? |
| **Stock** |
| View | ? | ? | ? | ? |
| Add | ? | ? | ? | ? |
| Remove | ? | ? | ? | ? |
| Adjust | ? | ? | ? | ? |
| Low Stock Alert | ? | ? | ? | ? |

---

## ? **Testing Checklist**

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

**?? Ready to integrate with Angular!**
