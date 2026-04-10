#!/usr/bin/env python3
"""
Извлекает данные о товарах из HTML файлов и создает JSON
"""

import os
import json
from pathlib import Path
from bs4 import BeautifulSoup
import re


# Курс конвертации RUB -> BYN (примерный курс на 2026)
RUB_TO_BYN = 0.035


def extract_price(price_text):
    """Извлекает числовое значение цены из текста"""
    if not price_text:
        return None
    # Убираем все кроме цифр и точки
    price_str = re.sub(r'[^\d.]', '', price_text)
    try:
        return float(price_str)
    except:
        return None


def parse_product_html(html_file):
    """Парсит HTML файл товара и извлекает данные"""
    try:
        with open(html_file, 'r', encoding='utf-8') as f:
            soup = BeautifulSoup(f.read(), 'html.parser')
        
        product = {}
        
        # Название товара (из h1)
        h1 = soup.find('h1')
        if h1:
            product['name'] = h1.get_text(strip=True)
        
        # Производитель
        manufacturer = soup.find('div', class_='manufacturer')
        if manufacturer:
            product['brandName'] = manufacturer.get_text(strip=True)
        else:
            product['brandName'] = 'Гейзер'  # По умолчанию
        
        # SKU (код товара)
        sku_div = soup.find('div', class_='sku')
        if sku_div:
            sku_text = sku_div.get_text(strip=True)
            sku_match = re.search(r'(\d+)', sku_text)
            if sku_match:
                product['sku'] = sku_match.group(1)
        
        # Цена
        price_reg = soup.find('p', class_='price-reg')
        price_new = soup.find('p', class_='price-new')
        price_old = soup.find('p', class_='price-old')
        
        if price_new:
            # Есть скидка
            price_rub = extract_price(price_new.get_text())
            if price_rub:
                product['price'] = round(price_rub * RUB_TO_BYN, 2)
            
            if price_old:
                old_price_rub = extract_price(price_old.get_text())
                if old_price_rub:
                    product['oldPrice'] = round(old_price_rub * RUB_TO_BYN, 2)
        elif price_reg:
            # Обычная цена
            price_rub = extract_price(price_reg.get_text())
            if price_rub:
                product['price'] = round(price_rub * RUB_TO_BYN, 2)
        
        # Описание (из мета-тега)
        meta_desc = soup.find('meta', attrs={'name': 'description'})
        if meta_desc and meta_desc.get('content'):
            product['shortDescription'] = meta_desc.get('content')
        
        # Полное описание (ищем в табах или основном контенте)
        description_parts = []
        tab_content = soup.find('div', id='tab-description')
        if tab_content:
            paragraphs = tab_content.find_all('p')
            for p in paragraphs:
                text = p.get_text(strip=True)
                if text:
                    description_parts.append(text)
        
        if description_parts:
            product['description'] = ' '.join(description_parts[:3])  # Первые 3 параграфа
        else:
            product['description'] = product.get('shortDescription', product.get('name', ''))
        
        # Изображения
        images = []
        # Главное изображение
        main_img = soup.find('img', class_='ms-brd')
        if main_img and main_img.get('data-src'):
            images.append(main_img['data-src'])
        elif main_img and main_img.get('src'):
            images.append(main_img['src'])
        
        # Дополнительные изображения
        thumbs = soup.find_all('img', class_='ms-thumb')
        for thumb in thumbs[:3]:  # Максимум 3 дополнительных
            if thumb.get('src') and thumb['src'] not in images:
                images.append(thumb['src'])
        
        product['imageUrls'] = images if images else []
        
        # Категория
        breadcrumbs = soup.find_all('li')
        for li in breadcrumbs:
            text = li.get_text(strip=True)
            if 'Питьевые фильтры под мойку' in text:
                product['categoryName'] = 'Питьевые фильтры под мойку'
                break
        
        if 'categoryName' not in product:
            product['categoryName'] = 'Фильтры для воды'
        
        # Тип фильтра (определяем по названию)
        name_lower = product.get('name', '').lower()
        if 'осмос' in name_lower:
            product['filterType'] = 1  # Обратный осмос
        elif 'проточн' in name_lower:
            product['filterType'] = 0  # Проточный
        else:
            product['filterType'] = 0  # По умолчанию проточный
        
        # Характеристики (примерные значения)
        product['stockQuantity'] = 10  # По умолчанию
        product['filterLifespanMonths'] = 12
        product['filterCapacityLiters'] = 4000
        product['flowRateLitersPerMinute'] = 2.5
        
        # Проверка наличия
        stock_span = soup.find('span', class_='instock')
        if stock_span:
            product['stockQuantity'] = 15
        
        return product
        
    except Exception as e:
        print(f"Ошибка при парсинге {html_file}: {e}")
        return None


def main():
    input_dir = Path('./GayzerItems')
    output_file = Path('./products-data.json')
    
    if not input_dir.exists():
        print(f"Ошибка: папка {input_dir} не найдена")
        return
    
    html_files = list(input_dir.glob('*.html'))
    print(f"Найдено HTML файлов: {len(html_files)}\n")
    
    products = []
    
    for i, html_file in enumerate(html_files, 1):
        print(f"[{i}/{len(html_files)}] Обработка: {html_file.name}")
        product = parse_product_html(html_file)
        if product:
            products.append(product)
            print(f"  OK: {product.get('name', 'Без названия')}")
        print()
    
    # Сохраняем в JSON
    with open(output_file, 'w', encoding='utf-8') as f:
        json.dump(products, f, ensure_ascii=False, indent=2)
    
    print(f"\nГотово! Создано товаров: {len(products)}")
    print(f"Файл сохранен: {output_file.absolute()}")


if __name__ == '__main__':
    main()
