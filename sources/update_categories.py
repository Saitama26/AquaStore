#!/usr/bin/env python3
"""
Определяет категории товаров по breadcrumbs в HTML файлах
"""

import json
from pathlib import Path
from bs4 import BeautifulSoup

def extract_category_from_html(html_file):
    """Извлекает категорию из breadcrumbs в HTML"""
    try:
        with open(html_file, 'r', encoding='utf-8') as f:
            soup = BeautifulSoup(f.read(), 'html.parser')
        
        # Ищем настоящие breadcrumbs (обычно в ul с классом breadcrumb или в определенном контейнере)
        # Ищем ul.breadcrumb или проверяем последовательность ссылок перед названием товара
        breadcrumb_container = soup.find('ul', class_='breadcrumb')
        
        if not breadcrumb_container:
            # Ищем breadcrumbs по структуре - последовательные li с ссылками перед названием товара
            # Обычно breadcrumbs идут перед основным контентом
            all_uls = soup.find_all('ul')
            for ul in all_uls:
                lis = ul.find_all('li', recursive=False)
                # Breadcrumbs обычно содержат 3-5 элементов
                if 3 <= len(lis) <= 6:
                    # Проверяем, есть ли в них ссылки на категории
                    has_category_links = False
                    for li in lis:
                        a = li.find('a')
                        if a and 'pitevye-filtry-pod-mojku' in str(a.get('href', '')):
                            has_category_links = True
                            break
                    
                    if has_category_links:
                        breadcrumb_container = ul
                        break
        
        if breadcrumb_container:
            breadcrumbs = breadcrumb_container.find_all('li')
            
            # Проверяем каждый элемент breadcrumb
            for li in breadcrumbs:
                a = li.find('a')
                if a and a.get('href'):
                    href = a['href']
                    text = a.get_text(strip=True)
                    
                    # Проверяем на тройную систему
                    if 'troynaya-sistema' in href:
                        return 'Тройная система'
                    
                    # Проверяем на обратный осмос
                    if 'obratnyy-osmos' in href:
                        return 'Обратный осмос'
        
        # Если не нашли в breadcrumbs, проверяем название товара
        h1 = soup.find('h1')
        if h1:
            name = h1.get_text(strip=True).lower()
            if 'осмос' in name or 'престиж' in name or 'allegro' in name.lower():
                return 'Обратный осмос'
        
        # По умолчанию тройная система
        return 'Тройная система'
        
    except Exception as e:
        print(f"Ошибка при обработке {html_file}: {e}")
        return 'Тройная система'

def update_products_categories():
    """Обновляет категории в products-data.json"""
    
    gayzer_dir = Path('./GayzerItems')
    products_file = Path('./products-data.json')
    
    # Читаем товары
    with open(products_file, 'r', encoding='utf-8') as f:
        products = json.load(f)
    
    print(f"Обработка {len(products)} товаров...\n")
    
    # Создаем маппинг SKU -> HTML файл
    html_files = list(gayzer_dir.glob('*.html'))
    
    categories_count = {
        'Обратный осмос': 0,
        'Тройная система': 0
    }
    
    # Обрабатываем каждый HTML файл
    for html_file in html_files:
        category = extract_category_from_html(html_file)
        
        # Читаем SKU из HTML
        with open(html_file, 'r', encoding='utf-8') as f:
            soup = BeautifulSoup(f.read(), 'html.parser')
        
        sku_div = soup.find('div', class_='sku')
        if sku_div:
            sku_text = sku_div.get_text(strip=True)
            import re
            sku_match = re.search(r'(\d+)', sku_text)
            if sku_match:
                sku = sku_match.group(1)
                
                # Находим товар с этим SKU
                for product in products:
                    if product.get('sku') == sku:
                        product['categoryName'] = category
                        categories_count[category] += 1
                        print(f"[{sku}] {product['name'][:50]}... -> {category}")
                        break
    
    # Сохраняем обновленные данные
    with open(products_file, 'w', encoding='utf-8') as f:
        json.dump(products, f, ensure_ascii=False, indent=2)
    
    print(f"\n=== СТАТИСТИКА ===")
    print(f"Обратный осмос: {categories_count['Обратный осмос']}")
    print(f"Тройная система: {categories_count['Тройная система']}")
    print(f"\nФайл обновлен: {products_file.absolute()}")

def create_categories_json():
    """Создает categories-data.json с 3 категориями"""
    
    categories = [
        {
            "name": "Питьевые фильтры под мойку",
            "description": "Системы очистки питьевой воды, устанавливаемые под мойку"
        },
        {
            "name": "Обратный осмос",
            "description": "Системы обратного осмоса для глубокой очистки воды"
        },
        {
            "name": "Тройная система",
            "description": "Трехступенчатые системы фильтрации воды"
        }
    ]
    
    categories_file = Path('./categories-data.json')
    with open(categories_file, 'w', encoding='utf-8') as f:
        json.dump(categories, f, ensure_ascii=False, indent=2)
    
    print(f"\nСоздан файл: {categories_file.absolute()}")
    print("Категории:")
    for cat in categories:
        print(f"  - {cat['name']}")

if __name__ == '__main__':
    update_products_categories()
    print("\n" + "="*50 + "\n")
    create_categories_json()
