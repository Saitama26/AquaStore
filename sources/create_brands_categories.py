#!/usr/bin/env python3
"""
Создает JSON файлы для брендов и категорий на основе products-data.json
"""

import json
from pathlib import Path

def create_brands_and_categories():
    # Читаем данные о товарах
    products_file = Path('./products-data.json')
    with open(products_file, 'r', encoding='utf-8') as f:
        products = json.load(f)
    
    # Извлекаем уникальные бренды
    brands_set = set()
    categories_set = set()
    
    for product in products:
        if 'brandName' in product:
            brands_set.add(product['brandName'])
        if 'categoryName' in product:
            categories_set.add(product['categoryName'])
    
    print(f"Найдено уникальных брендов: {len(brands_set)}")
    print(f"Найдено уникальных категорий: {len(categories_set)}\n")
    
    # Создаем данные для брендов
    brands = []
    brand_info = {
        'Гейзер': {
            'country': 'Россия',
            'description': 'Российский производитель систем очистки воды',
            'websiteUrl': 'https://geizer.com'
        },
        'Барьер': {
            'country': 'Россия',
            'description': 'Ведущий производитель фильтров для воды в России',
            'websiteUrl': 'https://barrier.ru'
        },
        'Atoll': {
            'country': 'США',
            'description': 'Американский производитель систем обратного осмоса',
            'websiteUrl': 'https://atoll-filter.ru'
        },
        'Новая Вода': {
            'country': 'Украина',
            'description': 'Производитель систем очистки воды',
            'websiteUrl': 'https://novavoda.ua'
        }
    }
    
    for brand_name in sorted(brands_set):
        brand = {
            'name': brand_name,
            'country': brand_info.get(brand_name, {}).get('country', 'Россия')
        }
        
        # Добавляем опциональные поля если есть
        if brand_name in brand_info:
            if 'description' in brand_info[brand_name]:
                brand['description'] = brand_info[brand_name]['description']
            if 'websiteUrl' in brand_info[brand_name]:
                brand['websiteUrl'] = brand_info[brand_name]['websiteUrl']
        
        brands.append(brand)
    
    # Создаем данные для категорий
    categories = []
    category_info = {
        'Питьевые фильтры под мойку': {
            'description': 'Системы очистки питьевой воды, устанавливаемые под мойку'
        },
        'Фильтры для воды': {
            'description': 'Различные системы фильтрации воды для дома и офиса'
        },
        'Обратный осмос': {
            'description': 'Системы обратного осмоса для глубокой очистки воды'
        }
    }
    
    for category_name in sorted(categories_set):
        category = {
            'name': category_name
        }
        
        # Добавляем опциональное описание если есть
        if category_name in category_info and 'description' in category_info[category_name]:
            category['description'] = category_info[category_name]['description']
        
        categories.append(category)
    
    # Сохраняем brands-data.json
    brands_file = Path('./brands-data.json')
    with open(brands_file, 'w', encoding='utf-8') as f:
        json.dump(brands, f, ensure_ascii=False, indent=2)
    
    print(f"Создан файл: {brands_file.absolute()}")
    print(f"Брендов: {len(brands)}\n")
    
    for brand in brands:
        print(f"  - {brand['name']} ({brand['country']})")
    
    # Сохраняем categories-data.json
    categories_file = Path('./categories-data.json')
    with open(categories_file, 'w', encoding='utf-8') as f:
        json.dump(categories, f, ensure_ascii=False, indent=2)
    
    print(f"\nСоздан файл: {categories_file.absolute()}")
    print(f"Категорий: {len(categories)}\n")
    
    for category in categories:
        print(f"  - {category['name']}")

if __name__ == '__main__':
    create_brands_and_categories()
