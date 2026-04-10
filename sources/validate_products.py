#!/usr/bin/env python3
"""
Проверяет и исправляет данные в products-data.json
"""

import json
from pathlib import Path

def validate_and_fix_products():
    input_file = Path('./products-data.json')
    
    with open(input_file, 'r', encoding='utf-8') as f:
        products = json.load(f)
    
    print(f"Всего товаров: {len(products)}\n")
    
    issues = []
    fixed_count = 0
    
    for i, product in enumerate(products):
        product_issues = []
        
        # Проверка цены
        if 'price' not in product:
            product_issues.append("Отсутствует цена")
        
        # Проверка shortDescription
        if 'shortDescription' not in product:
            product['shortDescription'] = product.get('description', product.get('name', ''))[:200]
            fixed_count += 1
            product_issues.append("Добавлено shortDescription")
        
        # Проверка filterType (должен быть 1 для обратного осмоса)
        name_lower = product.get('name', '').lower()
        if 'осмос' in name_lower and product.get('filterType') == 0:
            product['filterType'] = 1
            fixed_count += 1
            product_issues.append("Исправлен filterType на 1 (обратный осмос)")
        
        # Проверка наличия всех обязательных полей
        required_fields = ['name', 'price', 'brandName', 'categoryName', 'filterType']
        for field in required_fields:
            if field not in product:
                product_issues.append(f"Отсутствует поле: {field}")
        
        if product_issues:
            issues.append({
                'index': i,
                'name': product.get('name', 'Без названия'),
                'issues': product_issues
            })
    
    # Вывод отчета
    print("=== ОТЧЕТ О ПРОВЕРКЕ ===\n")
    
    if issues:
        print(f"Найдено проблем: {len(issues)}\n")
        for item in issues[:10]:  # Показываем первые 10
            print(f"[{item['index']}] {item['name']}")
            for issue in item['issues']:
                print(f"  - {issue}")
            print()
    else:
        print("Проблем не найдено!")
    
    print(f"\nИсправлено: {fixed_count}")
    
    # Сохраняем исправленный файл
    with open(input_file, 'w', encoding='utf-8') as f:
        json.dump(products, f, ensure_ascii=False, indent=2)
    
    print(f"\nФайл обновлен: {input_file.absolute()}")
    
    # Статистика
    print("\n=== СТАТИСТИКА ===")
    print(f"Товаров с ценами: {sum(1 for p in products if 'price' in p)}")
    print(f"Товаров с shortDescription: {sum(1 for p in products if 'shortDescription' in p)}")
    print(f"Проточные фильтры (filterType=0): {sum(1 for p in products if p.get('filterType') == 0)}")
    print(f"Обратный осмос (filterType=1): {sum(1 for p in products if p.get('filterType') == 1)}")

if __name__ == '__main__':
    validate_and_fix_products()
