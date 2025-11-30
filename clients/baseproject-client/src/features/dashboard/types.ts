export interface DashboardStatCard {
  title: string;
  value: number | string;
  description: string;
  icon: React.ComponentType<{ className?: string }>;
  trend?: {
    value: number;
    isPositive: boolean;
  };
  color?: 'blue' | 'green' | 'yellow' | 'purple' | 'red';
}

export interface RecentActivity {
  id: string;
  type: 'post_created' | 'post_updated' | 'post_deleted' | 'category_created';
  title: string;
  description: string;
  timestamp: Date;
  user?: string;
}

export interface ChartDataPoint {
  name: string;
  value: number;
  label?: string;
}

export interface InsightTrend {
  type: string;
  description: string;
  metric?: string;
  isPositive: boolean;
}

export interface InsightAlert {
  severity: 'low' | 'medium' | 'high' | 'critical';
  message: string;
  suggestion?: string;
}

export interface InsightRecommendation {
  category: string;
  title: string;
  description: string;
  actionUrl?: string;
  priority: number; // 1-5
}

export interface AiInsightsResponse {
  trends: InsightTrend[];
  alerts: InsightAlert[];
  recommendations: InsightRecommendation[];
}
