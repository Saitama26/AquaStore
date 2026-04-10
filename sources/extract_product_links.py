#!/usr/bin/env python3
"""
Извлекает ссылки на товары из HTML файлов и парсит их страницы
"""

import os
import sys
from pathlib import Path
from bs4 import BeautifulSoup
import requests
from urllib.parse import urljoin
import hashlib


def extract_product_links(html_file):
    """Извлекает ссылки на товары из HTML файла"""
    links = []
    
    try:
        with open(html_file, 'r', encoding='utf-8') as f:
            soup = BeautifulSoup(f.read(), 'html.parser')
        
        # Находим все элементы с классом product-thumb transition
        products = soup.find_all(class_='product-thumb transition')
        
        for product in products:
            # Ищем caption > h4 > a
            caption = product.find(class_='caption')
            if caption:
                h4 = caption.find('h4')
                if h4:
                    a = h4.find('a')
                    if a and a.get('href'):
                        links.append(a['href'])
        
        return links
        
    except Exception as e:
        print(f"Ошибка при чтении {html_file}: {e}")
        return []


def parse_product_page(url, output_dir, base_url='https://filterh2o.ru'):
    """Парсит страницу товара и сохраняет её"""
    try:
        # Формируем полный URL если нужно
        if not url.startswith('http'):
            url = urljoin(base_url, url)
        
        print(f"Парсинг: {url}")
        
        headers = {
            'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36'
        }
        response = requests.get(url, headers=headers, timeout=30)
        response.raise_for_status()
        
        soup = BeautifulSoup(response.content, 'html.parser')
        
        # Генерируем имя файла из URL
        url_hash = hashlib.md5(url.encode()).hexdigest()[:12]
        filename = f'product_{url_hash}.html'
        
        output_path = Path(output_dir) / filename
        with open(output_path, 'w', encoding='utf-8') as f:
            f.write(soup.prettify())
        
        print(f"[OK] Сохранено: {output_path}")
        return True
        
    except Exception as e:
        print(f"[ERROR] Ошибка: {e}")
        return False


def main():
    input_dir = Path('./outputData')
    output_dir = Path('./GayzerItems')
    
    # Создаем выходную папку
    output_dir.mkdir(parents=True, exist_ok=True)
    print(f"Выходная папка: {output_dir.absolute()}\n")
    
    # Собираем все ссылки из HTML файлов
    all_links = set()
    
    if not input_dir.exists():
        print(f"Ошибка: папка {input_dir} не найдена")
        return
    
    html_files = list(input_dir.glob('*.html'))
    print(f"Найдено HTML файлов: {len(html_files)}\n")
    
    for html_file in html_files:
        print(f"Анализ: {html_file.name}")
        links = extract_product_links(html_file)
        all_links.update(links)
        print(f"  Найдено ссылок: {len(links)}\n")
    
    print(f"Всего уникальных ссылок на товары: {len(all_links)}\n")
    
    # Парсим все найденные ссылки
    success_count = 0
    for i, link in enumerate(all_links, 1):
        print(f"[{i}/{len(all_links)}] ", end='')
        if parse_product_page(link, output_dir):
            success_count += 1
        print()
    
    print(f"\nЗавершено: {success_count}/{len(all_links)} успешно")


if __name__ == '__main__':
    main()
