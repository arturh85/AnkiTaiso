import { labels } from '@/i18n/ui';

const defaultLang = 'en';

type AllKeys<T> = T extends Record<infer K, infer V> ? keyof V : never;
type TranslationKey = AllKeys<typeof labels>;

export function useTranslations(lang: keyof typeof labels) {
  return function translate(key: TranslationKey) {
    return labels[lang][key] || labels[defaultLang][key];
  }
}