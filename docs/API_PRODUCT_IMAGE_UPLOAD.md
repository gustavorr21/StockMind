# API de Upload de Imagens de Produtos

## Base URL
```
https://localhost:49469/api/products
```

## Autenticação
Todos os endpoints requerem autenticação via token JWT no header:
```
Authorization: Bearer {seu-token-jwt}
```

---

## Upload de Imagem

### POST `/api/products/upload-image`

Faz upload de uma imagem de produto e retorna a URL para usar no cadastro.

#### Permissões
- ? Admin
- ? Manager
- ? Operator
- ? Viewer

#### Request
```http
POST /api/products/upload-image HTTP/1.1
Host: api.stockmind.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: multipart/form-data; boundary=----WebKitFormBoundary7MA4YWxkTrZu0gW

------WebKitFormBoundary7MA4YWxkTrZu0gW
Content-Disposition: form-data; name="file"; filename="produto.jpg"
Content-Type: image/jpeg

[binary data]
------WebKitFormBoundary7MA4YWxkTrZu0gW--
```

#### Validações
- ? **Tipos permitidos**: JPEG, JPG, PNG, GIF, WEBP
- ? **Tamanho máximo**: 5MB
- ? **Content-Type**: image/jpeg, image/png, image/gif, image/webp

#### Response Success (200 OK)
```json
{
  "imageUrl": "/uploads/products/3fa85f64-5717-4562-b3fc-2c963f66afa6.jpg"
}
```

#### Response Error (400 Bad Request)
```json
{
  "error": "No file uploaded"
}
```
ou
```json
{
  "error": "Invalid file type. Allowed types: JPEG, PNG, GIF, WEBP"
}
```
ou
```json
{
  "error": "File size exceeds the maximum allowed size of 5MB"
}
```

---

## Fluxo Completo: Criar Produto com Imagem

### Passo 1: Upload da Imagem
```typescript
async function uploadProductImage(token: string, file: File): Promise<string> {
  const formData = new FormData();
  formData.append('file', file);

  const response = await fetch('https://localhost:49469/api/products/upload-image', {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`
    },
    body: formData
  });

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.error);
  }

  const data = await response.json();
  return data.imageUrl;
}
```

### Passo 2: Criar Produto com a URL da Imagem
```typescript
async function createProductWithImage(
  token: string,
  productData: CreateProductCommand,
  imageFile?: File
): Promise<{ id: string }> {
  // Se houver imagem, faz upload primeiro
  let imageUrl = null;
  if (imageFile) {
    imageUrl = await uploadProductImage(token, imageFile);
  }

  // Cria o produto com a URL da imagem
  const response = await fetch('https://localhost:49469/products', {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      ...productData,
      imageUrl: imageUrl
    })
  });

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.error);
  }

  return await response.json();
}

// Uso:
const productData = {
  name: "Notebook Dell",
  description: "Notebook Dell Inspiron 15",
  sku: "DELL-NB-001",
  priceAmount: 3500.00,
  priceCurrency: "BRL",
  costPriceAmount: 2800.00,
  costPriceCurrency: "BRL",
  categoryId: "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  supplierId: null,
  minimumStock: 5,
  barcode: "7891234567890",
  imageUrl: null // Será preenchido após upload
};

const imageFile = document.querySelector('input[type="file"]').files[0];

try {
  const result = await createProductWithImage(token, productData, imageFile);
  console.log('Produto criado com ID:', result.id);
} catch (error) {
  console.error('Erro:', error.message);
}
```

---

## Exemplo Completo: React Component

```typescript
import { useState } from 'react';

interface ProductFormData {
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

export function ProductForm() {
  const [formData, setFormData] = useState<ProductFormData>({
    name: '',
    description: '',
    sku: '',
    priceAmount: 0,
    priceCurrency: 'BRL',
    costPriceAmount: 0,
    costPriceCurrency: 'BRL',
    categoryId: '',
    minimumStock: 0
  });
  const [imageFile, setImageFile] = useState<File | null>(null);
  const [imagePreview, setImagePreview] = useState<string | null>(null);
  const [uploading, setUploading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleImageChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      // Validação no frontend
      const validTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp'];
      if (!validTypes.includes(file.type)) {
        setError('Tipo de arquivo inválido. Use JPEG, PNG, GIF ou WEBP');
        return;
      }

      if (file.size > 5 * 1024 * 1024) {
        setError('Arquivo muito grande. Tamanho máximo: 5MB');
        return;
      }

      setImageFile(file);
      setImagePreview(URL.createObjectURL(file));
      setError(null);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setUploading(true);
    setError(null);

    try {
      const token = localStorage.getItem('token');
      if (!token) throw new Error('Não autenticado');

      // 1. Upload da imagem (se houver)
      let imageUrl = null;
      if (imageFile) {
        const formData = new FormData();
        formData.append('file', imageFile);

        const uploadResponse = await fetch('https://api.stockmind.com/api/products/upload-image', {
          method: 'POST',
          headers: {
            'Authorization': `Bearer ${token}`
          },
          body: formData
        });

        if (!uploadResponse.ok) {
          const error = await uploadResponse.json();
          throw new Error(error.error);
        }

        const uploadData = await uploadResponse.json();
        imageUrl = uploadData.imageUrl;
      }

      // 2. Criar produto com a URL da imagem
      const response = await fetch('https://api.stockmind.com/api/products', {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          ...formData,
          imageUrl: imageUrl
        })
      });

      if (!response.ok) {
        const error = await response.json();
        throw new Error(error.error);
      }

      const result = await response.json();
      alert(`Produto criado com sucesso! ID: ${result.id}`);
      
      // Limpar formulário
      setFormData({
        name: '',
        description: '',
        sku: '',
        priceAmount: 0,
        priceCurrency: 'BRL',
        costPriceAmount: 0,
        costPriceCurrency: 'BRL',
        categoryId: '',
        minimumStock: 0
      });
      setImageFile(null);
      setImagePreview(null);

    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erro desconhecido');
    } finally {
      setUploading(false);
    }
  };

  return (
    <form onSubmit={handleSubmit}>
      {/* Campos do formulário */}
      <input
        type="text"
        value={formData.name}
        onChange={(e) => setFormData({ ...formData, name: e.target.value })}
        placeholder="Nome do produto"
        required
      />

      {/* Upload de imagem */}
      <div>
        <label>Imagem do Produto:</label>
        <input
          type="file"
          accept="image/jpeg,image/jpg,image/png,image/gif,image/webp"
          onChange={handleImageChange}
        />
        {imagePreview && (
          <img 
            src={imagePreview} 
            alt="Preview" 
            style={{ maxWidth: '200px', marginTop: '10px' }} 
          />
        )}
      </div>

      {error && <div style={{ color: 'red' }}>{error}</div>}

      <button type="submit" disabled={uploading}>
        {uploading ? 'Salvando...' : 'Criar Produto'}
      </button>
    </form>
  );
}
```

---

## Axios Helper Functions

```typescript
import axios from 'axios';

const api = axios.create({
  baseURL: 'https://api.stockmind.com/api',
});

// Interceptor para adicionar token
api.interceptors.request.use(config => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Upload de imagem
export const uploadProductImage = async (file: File): Promise<string> => {
  const formData = new FormData();
  formData.append('file', file);

  const { data } = await api.post<{ imageUrl: string }>('/products/upload-image', formData, {
    headers: {
      'Content-Type': 'multipart/form-data'
    }
  });

  return data.imageUrl;
};

// Criar produto com imagem
export const createProductWithImage = async (
  productData: CreateProductCommand,
  imageFile?: File
) => {
  let imageUrl = null;

  if (imageFile) {
    imageUrl = await uploadProductImage(imageFile);
  }

  const { data } = await api.post<{ id: string }>('/products', {
    ...productData,
    imageUrl
  });

  return data;
};

// Atualizar produto com nova imagem
export const updateProductWithImage = async (
  id: string,
  productData: UpdateProductCommand,
  imageFile?: File
) => {
  let imageUrl = productData.imageUrl;

  // Se houver nova imagem, faz upload
  if (imageFile) {
    imageUrl = await uploadProductImage(imageFile);
  }

  await api.put(`/products/${id}`, {
    ...productData,
    imageUrl
  });
};
```

---

## Notas Importantes

1. **Armazenamento Local**: As imagens são salvas na pasta `wwwroot/uploads/products/` no servidor.

2. **Nome do Arquivo**: Um GUID único é gerado para cada imagem para evitar conflitos.

3. **URL Retornada**: A URL retornada é relativa (ex: `/uploads/products/3fa85f64...jpg`).

4. **Acesso às Imagens**: As imagens podem ser acessadas diretamente via:
   ```
   https://api.stockmind.com/uploads/products/3fa85f64-5717-4562-b3fc-2c963f66afa6.jpg
   ```

5. **Segurança**: 
   - Apenas Admin e Manager podem fazer upload
   - Validação de tipo de arquivo no backend
   - Limite de tamanho: 5MB

6. **Fluxo Recomendado**:
   - Upload da imagem ANTES de criar/atualizar o produto
   - Usar a URL retornada no campo `imageUrl`

7. **Produção**: Para produção, considere migrar para Azure Blob Storage, AWS S3 ou Cloudinary.

---

## Códigos de Status HTTP

| Código | Descrição |
|--------|-----------|
| 200 | OK - Upload realizado com sucesso |
| 400 | Bad Request - Arquivo inválido ou muito grande |
| 401 | Unauthorized - Token ausente ou inválido |
| 403 | Forbidden - Usuário sem permissão |
| 500 | Internal Server Error - Erro no servidor |
