#!/usr/bin/env python3
"""
Отладка определения категорий
"""

from pathlib import Path
from bs4 import BeautifulSoup

def debug_category(html_file):
    """Отладка определения категории"""
    with open(html_file, 'r', encoding='utf-8') as f:
        soup = BeautifulSoup(f.read(), 'html.parser')
    
    print(f"\n=== {html_file.name} ===")
    
    # Ищем breadcrumbs
    breadcrumbs = soup.find_all('li')
    print(f"Найдено <li> элементов: {len(breadcrumbs)}")
    
    for i, li in enumerate(breadcrumbs[:15]):
        a = li.find('a')
        if a and a.get('href'):
            href = a['href']
            text = a.get_text(strip=True)
            print(f"  [{i}] {text[:40]} -> {href}")
            
            if 'troynaya-sistema' in href:
                print(f"      ^^^ НАЙДЕНА ТРОЙНАЯ СИСТЕМА!")
            if 'obratnyy-osmos' in href:
                print(f"      ^^^ НАЙДЕН ОБРАТНЫЙ ОСМОС!")

# Тестируем несколько файлов
gayzer_dir = Path('./GayzerItems')
test_files = list(gayzer_dir.glob('*.html'))[:5]

for html_file in test_files:
    debug_category(html_file)
